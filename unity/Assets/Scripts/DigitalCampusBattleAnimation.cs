using UnityEngine;

public sealed partial class DigitalCampusGame
{
    private bool turnAnimating, playerImpact, enemyImpact;
    private float turnTime;
    private int turnAction, visibleBugHp, visiblePlayerHp, bugDamage, playerDamage, healing;
    private string turnResult;
    private const float PlayerImpactAt = .55f, EnemyImpactAt = 1.45f;

    private void ResetBattleTurn()
    {
        turnAnimating = playerImpact = enemyImpact = false;
        turnTime = battleAnimation = 0;
        visibleBugHp = battle.BugHp; visiblePlayerHp = battle.PlayerHp;
    }
    private void BeginAnimatedTurn(int action)
    {
        if (turnAnimating || battle.Won || battle.Lost) return;
        int oldTurn = battle.Turn, oldBug = battle.BugHp, oldPlayer = battle.PlayerHp;
        turnResult = battle.Act(action);
        if (battle.Turn == oldTurn) { feedback = turnResult; return; }
        turnAction = action; turnTime = 0; turnAnimating = true;
        playerImpact = enemyImpact = false;
        bugDamage = oldBug - battle.BugHp;
        healing = action == 3 ? Mathf.Min(BugBattle.PatchHealing, BugBattle.PlayerMaxHp - oldPlayer) : 0;
        playerDamage = oldPlayer + healing - battle.PlayerHp;
        feedback = new[] { "Debug! ส่งพลังค้นหาข้อผิดพลาด", "Test! ยิงชุดทดสอบตรวจระบบ", "Pair Programming! สร้างเกราะทีม", "Patch! ฟื้นฟูระบบและ HP" }[action];
        PlayBattleAction(action, false);
    }
    private void TickBattleTurn()
    {
        if (!turnAnimating) return;
        turnTime += Time.deltaTime;
        if (!playerImpact && turnTime >= PlayerImpactAt)
        {
            playerImpact = true; visibleBugHp = battle.BugHp; visiblePlayerHp += healing;
            if (bugDamage > 0) PlayBattleAction(0, false);
            feedback = healing > 0 ? "ฟื้น HP +" + healing : "โจมตีโดน! น้องบั๊ก HP −" + bugDamage;
        }
        if (!battle.Won && !enemyImpact && turnTime >= EnemyImpactAt)
        {
            enemyImpact = true; visiblePlayerHp = battle.PlayerHp;
            PlayBattleAction(0, false);
            feedback = turnAction == 2 ? "เกราะทีมช่วยลดแรงโจมตี! HP −" + playerDamage : "น้องบั๊กกระโดดโจมตีสวน! HP −" + playerDamage;
        }
        if (turnTime >= (battle.Won ? 1.15f : 2.05f))
        {
            turnAnimating = false; visiblePlayerHp = battle.PlayerHp; visibleBugHp = battle.BugHp;
            feedback = turnResult;
            if (battle.Won) PlayBattleAction(0, true);
        }
    }
    private void DrawBattleActors(float bob)
    {
        float lunge = turnAnimating && turnTime < .65f ? Mathf.Sin(Mathf.Clamp01(turnTime / .65f) * Mathf.PI) : 0;
        float counter = turnAnimating && !battle.Won && turnTime >= .9f && turnTime < 1.6f
            ? Mathf.Sin((turnTime - .9f) / .7f * Mathf.PI) : 0;
        float bugHit = turnAnimating ? HitPulse(turnTime - PlayerImpactAt) : 0;
        float playerHit = turnAnimating && !battle.Won ? HitPulse(turnTime - EnemyImpactAt) : 0;
        Rect hero = new Rect(220 + lunge * 80 - playerHit * 22, 280 + bob - lunge * 26, 230, 230);
        Rect bug = new Rect(770 - counter * 140 + bugHit * 22, 100 - bob - counter * 72, 360, 225);
        if (walkSprites.Length >= 24)
        {
            // Back-facing walk frames keep the actor facing the opponent while lunging.
            int frame = turnAnimating && turnTime < .65f ? 18 + Mathf.FloorToInt(turnTime * 14) % 6 : 18;
            DrawCombatSprite(walkSprites[frame], hero, playerHit > 0);
        }
        if (bugSprites.Length > 0)
        {
            Sprite sprite = bugSprites[Mathf.FloorToInt(battleAnimation * 6) % bugSprites.Length];
            if (counter > 0 && bugAttackSprites.Length > 0)
                sprite = bugAttackSprites[Mathf.Clamp(Mathf.FloorToInt((turnTime - .9f) / .7f * bugAttackSprites.Length), 0, bugAttackSprites.Length - 1)];
            DrawCombatSprite(sprite, bug, bugHit > 0 && bugDamage > 0);
        }
        if (turnAnimating && turnTime > PlayerImpactAt && turnTime < 1.1f && bugDamage > 0)
            FloatingNumber(new Vector2(959, 186), "−" + bugDamage, turnTime - PlayerImpactAt, new Color(1, .82f, .35f));
        if (turnAnimating && turnTime > EnemyImpactAt && playerDamage > 0)
            FloatingNumber(new Vector2(330, 322), "−" + playerDamage, turnTime - EnemyImpactAt, new Color(1, .5f, .45f));
        if (turnAnimating && healing > 0 && turnTime > .25f && turnTime < 1.1f)
            FloatingNumber(new Vector2(340, 292), "+" + healing + " HP", turnTime - .25f, new Color(.4f, 1, .65f));
    }
    private static float HitPulse(float elapsed)
    {
        return elapsed >= 0 && elapsed < .36f ? Mathf.Sin(elapsed * 65) * (1 - elapsed / .36f) : 0;
    }
    private static void DrawCombatSprite(Sprite sprite, Rect rect, bool hit)
    {
        Color saved = GUI.color;
        if (hit) GUI.color = new Color(1, .35f, .35f);
        DrawSprite(sprite, rect); GUI.color = saved;
    }
    private void FloatingNumber(Vector2 position, string text, float elapsed, Color color)
    {
        Color saved = GUI.color; GUI.color = color;
        CampusOutlinedText.Label(new Rect(position.x - 45, position.y - elapsed * 75, 190, 40), text, headingStyle);
        GUI.color = saved;
    }
    private void DrawTurnEffects()
    {
        if (!turnAnimating) return;
        Vector2 hero = new Vector2(355, 375), bug = new Vector2(949, 236);
        Color cyan = new Color(.4f, .9f, 1), red = new Color(1, .4f, .4f);
        if (turnAction < 2 && turnTime >= .18f && turnTime < PlayerImpactAt)
        {
            float progress = (turnTime - .18f) / (PlayerImpactAt - .18f);
            DrawProjectile(hero, bug, progress, turnAction == 0 ? cyan : new Color(.85f, .6f, 1));
        }
        if (turnAction == 2 && turnTime < 1.85f)
        {
            float pulse = 1 + Mathf.Sin(turnTime * 12) * .04f;
            Disc(new Rect(hero.x - 84 * pulse, hero.y - 100 * pulse, 168 * pulse, 190 * pulse), new Color(.4f, .85f, 1, .23f));
            for (int i = 0; i < 12; i++)
            {
                float a = i * Mathf.PI / 6 + turnTime * 2;
                Vector2 point = hero + new Vector2(Mathf.Cos(a) * 88, Mathf.Sin(a) * 94);
                Fill(new Rect(point.x - 3, point.y - 3, 6, 6), cyan);
            }
        }
        if (turnAction == 3 && turnTime < 1.05f)
        {
            for (int i = 0; i < 6; i++)
            {
                float phase = Mathf.Repeat(turnTime * .9f + i / 6f, 1);
                Vector2 point = hero + new Vector2(Mathf.Sin(i * 2.7f) * 68, 70 - phase * 170);
                Color green = new Color(.4f, 1, .65f, Mathf.Sin(phase * Mathf.PI));
                Fill(new Rect(point.x - 10, point.y - 3, 20, 6), green);
                Fill(new Rect(point.x - 3, point.y - 10, 6, 20), green);
            }
        }
        if(turnAnimating && turnAction<2 && turnTime>=PlayerImpactAt && turnTime<PlayerImpactAt+.3f) {
            float t=(turnTime-PlayerImpactAt)/.3f;
            Color slash=new Color(.7f,1,.9f,1-t);
            Line(bug+new Vector2(-60,-55)*t,bug+new Vector2(65,60)*t,slash,8*(1-t)+2);
            Line(bug+new Vector2(-45,60)*t,bug+new Vector2(50,-50)*t,slash,6*(1-t)+2);
        }
        if (bugDamage > 0) ImpactBurst(bug, turnTime - PlayerImpactAt, cyan);
        if (!battle.Won && turnTime >= 1.12f && turnTime < EnemyImpactAt)
            DrawProjectile(bug, hero, (turnTime - 1.12f) / (EnemyImpactAt - 1.12f), red);
        if (!battle.Won) ImpactBurst(hero, turnTime - EnemyImpactAt, turnAction == 2 ? cyan : red);
    }
    private void DrawProjectile(Vector2 start, Vector2 end, float progress, Color color)
    {
        for (int i = 5; i >= 0; i--)
        {
            Vector2 point = Vector2.Lerp(start, end, Mathf.Clamp01(progress - i * .035f));
            float size = i == 0 ? 28 : 15 - i;
            color.a = 1 - i * .13f;
            Disc(new Rect(point.x - size / 2, point.y - size / 2, size, size), color);
        }
    }
    private void ImpactBurst(Vector2 center, float elapsed, Color color)
    {
        if (elapsed < 0 || elapsed > .42f) return;
        float t = elapsed / .42f; color.a = 1 - t;
        for (int i = 0; i < 10; i++)
        {
            float angle = i * Mathf.PI / 5;
            Vector2 axis = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Line(center + axis * (12 + t * 25), center + axis * (30 + t * 65), color, 5 * (1 - t) + 1);
        }
    }
}
