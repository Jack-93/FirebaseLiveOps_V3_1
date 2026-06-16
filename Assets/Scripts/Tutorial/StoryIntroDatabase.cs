using System.Collections.Generic;

public static class StoryIntroDatabase
{
    public const string PlayerRole = "참새 이등병";
    public const string EnemyFaction = "고양이";
    public const string WarObjective = "전봇대";
    public const string VisualStyle =
        "귀엽고 둥근 픽셀 아트. 작은 실루엣, 밝은 색감, 두꺼운 도트 외곽선.";

    private static readonly List<StoryIntroCut> Cuts =
        new List<StoryIntroCut>
        {
            new StoryIntroCut(
                1,
                "전봇대 위의 세상",
                "대사는 추후 확정",
                "평화로운 동네 전봇대 위에 작은 새들이 모여 있는 귀여운 픽셀 아트 장면. (아트 필요)",
                "#87C7FF"),
            new StoryIntroCut(
                2,
                "고양이의 진격",
                "대사는 추후 확정",
                "골목 아래에서 고양이 부대가 전봇대를 올려다보는 긴장감 있는 픽셀 아트 장면. (아트 필요)",
                "#F5A64A"),
            new StoryIntroCut(
                3,
                "전봇대 쟁탈전",
                "대사는 추후 확정",
                "전봇대를 중심으로 새 부대와 고양이 부대가 대치하는 만화 컷. (아트 필요)",
                "#D96B6B"),
            new StoryIntroCut(
                4,
                "참새 이등병",
                "대사는 추후 확정",
                "작은 참새 병사가 붕대를 감은 왼쪽 날개를 내려다보는 귀여우면서도 짠한 픽셀 아트 장면. (아트 필요)",
                "#B9D4E8"),
            new StoryIntroCut(
                5,
                "뒤에서 할 수 있는 일",
                "대사는 추후 확정",
                "참새 이등병이 작은 발전기나 배터리 장치 앞에 서 있는 장면. (아트 필요)",
                "#FFE082"),
            new StoryIntroCut(
                6,
                "스킬 충전!",
                "대사는 추후 확정",
                "참새 이등병이 전기를 충전하고, 동료 새들의 스킬 게이지가 빛나는 픽셀 아트 장면. (아트 필요)",
                "#7AE0D6"),
            new StoryIntroCut(
                7,
                "작전 개시",
                "대사는 추후 확정",
                "전봇대 위에 모인 조류 팀이 출격 준비를 하는 밝고 결의 있는 픽셀 아트 장면. (아트 필요)",
                "#9BE36D")
        };

    public static IReadOnlyList<StoryIntroCut> GetCuts()
    {
        return Cuts;
    }
}
