namespace Zongzu.Contracts;

/// <summary>
/// PERSON_OWNERSHIP_RULES.md / LIVING_WORLD_DESIGN.md §2.2 — a clan member's
/// structural position inside the lineage, independent of age and office.
///
/// <para>This is the <b>family-side</b> answer to "who is this person in
/// the clan?"; it does not describe what they <i>do</i>. Office, trade,
/// study, and force assignments live in their respective modules per
/// <c>PERSON_OWNERSHIP_RULES.md</c>.</para>
///
/// <list type="bullet">
///   <item><see cref="MainLineHeir"/> — 长房嫡嗣，祠堂香火的第一顺位。</item>
///   <item><see cref="BranchHead"/> — 旁房掌事之人（房长）。</item>
///   <item><see cref="BranchMember"/> — 旁房在籍族众。</item>
///   <item><see cref="DependentKin"/> — 寡妇、孤侄、未成年依附人口。</item>
///   <item><see cref="MarriedOut"/> — 已出嫁女，族谱录名、权威在外家。</item>
/// </list>
/// </summary>
public enum BranchPosition
{
    Unknown = 0,

    MainLineHeir = 1,

    BranchHead = 2,

    BranchMember = 3,

    DependentKin = 4,

    MarriedOut = 5,
}
