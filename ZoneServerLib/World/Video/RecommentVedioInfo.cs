using EnumerateUtility;

public class RecommentVedioInfo
{
    public int LadderLevel { get; private set; }
    public VedioFilterType FilterType { get; private set; }
    public RecommentVedioType RecommentType { get; private set; }

    public RecommentVedioInfo(RecommentVedioType recomment_type, VedioFilterType filter_type, int ladder_level)
    {
        RecommentType = recomment_type;
        FilterType = filter_type;
        LadderLevel = ladder_level;
    }
}
