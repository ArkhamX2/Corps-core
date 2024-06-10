using MegaCorps.Core.Model.Enums;

public class CardDescriptionComparer : IComparer<CardDescriptionInfo>
{
    public int Compare(CardDescriptionInfo x, CardDescriptionInfo y)
    {
        if (x == null || y == null)
        {
            return 0;
        }

        return x.Id.CompareTo(y.Id);
    }
}

public class AttackTypeComparer : IEqualityComparer<AttackType>
{
    public bool Equals(AttackType x, AttackType y)
    {
        return x == y;
    }

    public int GetHashCode(AttackType obj)
    {
        return obj.ToString().GetHashCode() ^ obj.ToString().GetHashCode();
    }
}


