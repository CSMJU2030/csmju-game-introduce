using System;
using System.Collections.Generic;

// Rules are independent of rendering; GUI events never advance a game timer.
public sealed class WirePuzzle
{
    public readonly int[] Cells = { 1, 2, 3, 4, 5, 6, 7, 8, 0 };
    public float Remaining = 90f;
    public int Moves;
    // North, east, south, west. A route may be valid without every tile being ordered.
    private static readonly int[] Ports = { 0, 10, 10, 12, 6, 10, 9, 3, 12 };
    public bool Move(int index)
    {
        if (index < 0 || index >= 9 || Remaining <= 0) return false;
        int blank = Array.IndexOf(Cells, 0);
        if (Math.Abs(index / 3 - blank / 3) + Math.Abs(index % 3 - blank % 3) != 1) return false;
        Cells[blank] = Cells[index]; Cells[index] = 0; Moves++; return true;
    }
    public bool Connected()
    {
        if ((Ports[Cells[0]] & 8) == 0 || (Ports[Cells[7]] & 4) == 0) return false;
        var queue = new Queue<int>(); var seen = new HashSet<int>(); queue.Enqueue(0); seen.Add(0);
        int[] dx = { 0, 1, 0, -1 }, dy = { -1, 0, 1, 0 }, bits = { 1, 2, 4, 8 };
        while (queue.Count > 0)
        {
            int cell = queue.Dequeue(); if (cell == 7) return true;
            for (int d = 0; d < 4; d++)
            {
                int x = cell % 3 + dx[d], y = cell / 3 + dy[d];
                if (x < 0 || x > 2 || y < 0 || y > 2) continue;
                int next = y * 3 + x;
                if ((Ports[Cells[cell]] & bits[d]) != 0 && (Ports[Cells[next]] & bits[(d + 2) % 4]) != 0 && seen.Add(next)) queue.Enqueue(next);
            }
        }
        return false;
    }
    public int PortAt(int index) => Ports[Cells[index]];
    public void Shuffle(Random random)
    {
        do {
            Remaining = 90f;
            for (int i = 0; i < 9; i++) Cells[i] = (i + 1) % 9;
            int previous = -1;
            for (int n = 0; n < 24; n++)
            {
                int blank = Array.IndexOf(Cells, 0); var choices = new List<int>();
                for (int i = 0; i < 9; i++) if (i != previous && Math.Abs(i / 3 - blank / 3) + Math.Abs(i % 3 - blank % 3) == 1) choices.Add(i);
                Move(choices[random.Next(choices.Count)]); previous = blank;
            }
        } while (Connected());
        Moves = 0; Remaining = 90;
    }
}

public sealed class CipherPuzzle
{
    public int Shift, Rotation;
    public string Plaintext = "DATA";
    public static string Transform(string text, int shift)
    {
        char[] output = text.ToCharArray();
        for (int i = 0; i < output.Length; i++) output[i] = (char)('A' + ((output[i] - 'A' + shift) % 26 + 26) % 26);
        return new string(output);
    }
    public string Encoded => Transform(Plaintext, Shift);
    public string Decoded => Transform(Encoded, -Rotation);
    public bool Solved => Decoded == Plaintext;
    public void Rotate(int delta) { Rotation = ((Rotation + delta) % 26 + 26) % 26; }
}

public sealed class BugBattle
{
    public const int PlayerMaxHp = 20, BugMaxHp = 28, MaxEnergy = 5, PatchHealing = 8;
    public int PlayerHp = PlayerMaxHp, BugHp = BugMaxHp, Energy = MaxEnergy, Patches = 3, Turn;
    public int Intent;
    public bool Enraged => BugHp <= BugMaxHp / 2;
    public bool Won => BugHp <= 0;
    public bool Lost => PlayerHp <= 0;
    public readonly Random Random;
    public BugBattle(int seed) { Random = new Random(seed); Intent = Random.Next(3); }
    public string Act(int action)
    {
        if (action < 0 || action > 3 || Won || Lost) return "การต่อสู้จบแล้ว";
        bool guard = action == 2;
        if (action < 2 && Energy < 2) return "ต้องใช้พลัง 2! ใช้ Pair Programming เพื่อฟื้นพลัง";
        if (action == 3 && Patches == 0) return "ใช้ Patch หมดแล้ว";
        int oldIntent = Intent;
        string message;
        if (action < 2) {
            Energy -= 2; int damage = (action == 0 && Intent == 0 || action == 1 && Intent == 2) ? 6 : 3;
            BugHp = Math.Max(0, BugHp - damage); message = "เจาะจุดอ่อนปีศาจ! HP −" + damage;
        }
        else if (guard) { BugHp = Math.Max(0, BugHp - 2); Energy = Math.Min(MaxEnergy, Energy + 3); message = "ทีมช่วยตรวจโค้ด! ฟื้นพลัง +3 และตั้งเกราะ"; }
        else { Patches--; PlayerHp = Math.Min(PlayerMaxHp, PlayerHp + PatchHealing); message = "ติดตั้ง Patch ฟื้น HP +8"; }
        Turn++;
        if (Won) return message + " · ชนะแล้ว!";
        int hit = guard ? 1 : (oldIntent == 2 ? 5 : oldIntent == 1 ? 4 : 3) + (Enraged ? 1 : 0);
        PlayerHp = Math.Max(0, PlayerHp - hit);
        if (oldIntent == 1 && !guard) { Energy = Math.Max(0, Energy - 1); message += " · Memory Leak ดูดพลัง −1"; }
        Intent = Random.Next(3);
        return message + "\nปีศาจบั๊กโต้กลับ HP −" + hit + (Enraged ? " · โหมดคลั่ง!" : "") + (Lost ? " · ทีมต้องพักฟื้น" : "");
    }
}
