import { SubsystemRole } from './core-hub-identity';

/**
 * Subsystem permissions (spec §16).
 *
 *   Core JWT -> Core Role -> Subsystem Role -> Permission -> Business Operation
 *
 * Business code asks for a permission, never for `role === 'admin'`.
 * `:own` variants are scope hints: the guard lets the request through and the
 * service performs the ownership check against business data.
 */
export enum Permission {
  /**
   * Game content themselves are Core Hub reference data: they are added and closed in
   * the Core Hub backoffice, so this subsystem has no room:create/update.
   */
  GAME_READ = 'game:read',

  GAME_SESSION_READ_ANY = 'game-session:read:any',
  GAME_SESSION_READ_OWN = 'game-session:read:own',
  GAME_SESSION_CREATE = 'game-session:create',
  GAME_SESSION_CANCEL_ANY = 'game-session:cancel:any',
  GAME_SESSION_CANCEL_OWN = 'game-session:cancel:own',
  /** Approve or reject a pending game session. */
  GAME_SESSION_REVIEW = 'game-session:review',
}

/** Students book game content for themselves and follow their own requests. */
const STUDENT_PERMISSIONS: Permission[] = [
  Permission.GAME_READ,
  Permission.GAME_SESSION_READ_OWN,
  Permission.GAME_SESSION_CREATE,
  Permission.GAME_SESSION_CANCEL_OWN,
];

/** Alumni may look at game content but not start sessions. */
const ALUMNI_PERMISSIONS: Permission[] = [Permission.GAME_READ];

/** Staff run the game sessions: they see and review every request. */
const STAFF_PERMISSIONS: Permission[] = [
  Permission.GAME_READ,
  Permission.GAME_SESSION_READ_ANY,
  Permission.GAME_SESSION_READ_OWN,
  Permission.GAME_SESSION_CREATE,
  Permission.GAME_SESSION_CANCEL_ANY,
  Permission.GAME_SESSION_CANCEL_OWN,
  Permission.GAME_SESSION_REVIEW,
];

const ADMIN_PERMISSIONS: Permission[] = Object.values(Permission);

export const ROLE_PERMISSIONS: Readonly<Record<SubsystemRole, readonly Permission[]>> =
  Object.freeze({
    [SubsystemRole.STUDENT]: Object.freeze(STUDENT_PERMISSIONS),
    [SubsystemRole.ALUMNI]: Object.freeze(ALUMNI_PERMISSIONS),
    [SubsystemRole.STAFF]: Object.freeze(STAFF_PERMISSIONS),
    [SubsystemRole.ADMIN]: Object.freeze(ADMIN_PERMISSIONS),
  });

/** Does this subsystem role hold the given permission? */
export function can(role: SubsystemRole, permission: Permission): boolean {
  return ROLE_PERMISSIONS[role]?.includes(permission) ?? false;
}

/** Does this subsystem role hold at least one of the given permissions? */
export function canAny(role: SubsystemRole, permissions: readonly Permission[]): boolean {
  return permissions.some((permission) => can(role, permission));
}
