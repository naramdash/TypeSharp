namespace TypeSharp.Compiler.Interop;

public sealed record CSharpOverloadResolution(
    IReadOnlyList<CSharpOverloadCandidate> ApplicableCandidates,
    IReadOnlyList<CSharpOverloadCandidate> BestCandidates)
{
    public bool IsAmbiguous => BestCandidates.Count > 1;

    public CSharpOverloadCandidate? SelectedCandidate => BestCandidates.Count == 1 ? BestCandidates[0] : null;
}
