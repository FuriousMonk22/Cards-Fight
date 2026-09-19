using UnityEngine;

public class one_up : Ability
{
    public one_up(Creature owner) : base(owner)
    {
    }

    public override void OnDeath()
    {
        Vector3Int origin = owner.cell;
        string creatureName = owner.creatureData.Name;
        int bestDistance = int.MaxValue;
        Vector3Int bestCell = origin;

        for (int x = origin.x - 1; x <= origin.x + 1; x++)
        {
            for (int y = origin.y - 1; y <= origin.y + 1; y++)
            {
                if (!owner.creaturesGrid.IsInsideBounds(x, y) ||
                    owner.creaturesGrid.Creatures[x, y] != null)
                    continue;

                int distance = Mathf.Abs(x - origin.x) + Mathf.Abs(y - origin.y);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestCell = new Vector3Int(x, y, 0);
                }
            }
        }

        if (bestDistance != int.MaxValue &&
            owner.creaturesGrid.Spawn(creatureName, bestCell.x, bestCell.y, true))
        {
            Creature respawned = owner.creaturesGrid.Creatures[bestCell.x, bestCell.y]
                .GetComponent<Creature>();
            respawned.team = owner.team;
        }

        owner.abilities.Remove(this);
    }
}
