namespace System.Diagnostics.CodeAnalysis;

// This class does not exist in .NET Framework 4.6.1, so we need to define it here.
internal class MemberNotNullAttribute : Attribute
{
    public string[] Members { get; }
    public MemberNotNullAttribute(string member)
    {
        Members = new string[] { member };
    }
    public MemberNotNullAttribute(string[] members)
    {
        Members = members;
    }
}