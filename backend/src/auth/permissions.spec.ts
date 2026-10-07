import { SubsystemRole } from './core-hub-identity';
import { Permission, ROLE_PERMISSIONS, can, canAny } from './permissions';

describe('Subsystem permission model (spec §15, §16)', () => {
  describe('STUDENT', () => {
    const role = SubsystemRole.STUDENT;

    it('can see game content, book one and follow or cancel its own game sessions', () => {
      expect(can(role, Permission.GAME_READ)).toBe(true);
      expect(can(role, Permission.GAME_SESSION_CREATE)).toBe(true);
      expect(can(role, Permission.GAME_SESSION_READ_OWN)).toBe(true);
      expect(can(role, Permission.GAME_SESSION_CANCEL_OWN)).toBe(true);
    });

    it("cannot review game sessions or touch other people's game sessions", () => {
      expect(can(role, Permission.GAME_SESSION_REVIEW)).toBe(false);
      expect(can(role, Permission.GAME_SESSION_READ_ANY)).toBe(false);
      expect(can(role, Permission.GAME_SESSION_CANCEL_ANY)).toBe(false);
    });
  });

  describe('ALUMNI', () => {
    it('can only look at game content', () => {
      const role = SubsystemRole.ALUMNI;
      expect(can(role, Permission.GAME_READ)).toBe(true);
      expect(can(role, Permission.GAME_SESSION_CREATE)).toBe(false);
      expect(can(role, Permission.GAME_SESSION_READ_OWN)).toBe(false);
    });
  });

  describe('STAFF', () => {
    it('sees game content and reviews every game session', () => {
      const role = SubsystemRole.STAFF;
      expect(can(role, Permission.GAME_READ)).toBe(true);
      expect(can(role, Permission.GAME_SESSION_READ_ANY)).toBe(true);
      expect(can(role, Permission.GAME_SESSION_REVIEW)).toBe(true);
    });
  });

  describe('ADMIN', () => {
    it('holds every permission', () => {
      for (const permission of Object.values(Permission)) {
        expect(can(SubsystemRole.ADMIN, permission)).toBe(true);
      }
    });
  });

  it('canAny passes when at least one permission matches', () => {
    expect(
      canAny(SubsystemRole.STUDENT, [Permission.GAME_SESSION_READ_ANY, Permission.GAME_SESSION_READ_OWN]),
    ).toBe(true);
    expect(
      canAny(SubsystemRole.ALUMNI, [Permission.GAME_SESSION_CREATE, Permission.GAME_SESSION_REVIEW]),
    ).toBe(false);
  });

  it('defines permissions for every subsystem role', () => {
    for (const role of Object.values(SubsystemRole)) {
      expect(ROLE_PERMISSIONS[role]).toBeDefined();
    }
  });
});
