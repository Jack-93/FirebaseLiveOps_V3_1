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
                "참새 비행단",
                "오늘도 마을 전봇대를 평화롭게 지키는 참새 비행단. 나는 그곳의 이등병이다!",
                "평화로운 전봇대 위 참새 비행단과, 선글라스를 쓴 비행단장이 멋진 폼으로 날아가는 장면. (아트 필요)",
                "#87C7FF",
                artResourcePath: "PrototypeArt/Story/IntroCut01"),
            new StoryIntroCut(
                2,
                "겁쟁이 이등병",
                "나도 단장님처럼 멋진 썬구리 참새가 되고 싶다. 사실 벌레도, 산책하는 강아지도 너무 무섭지만!",
                "작은 참새 이등병이 겁먹으면서도 선글라스를 닦고 훈련하는 귀여운 만화 컷. (아트 필요)",
                "#F5A64A",
                artResourcePath: "PrototypeArt/Story/IntroCut02"),
            new StoryIntroCut(
                3,
                "공습경보",
                "큰일이다! 갑자기 고양이들이 몰려온다! 모두 날개를 펴고 대피해라!",
                "검은 경보 화면, 다급한 말풍선, 전봇대 아래로 몰려오는 고양이들의 실루엣. (아트 필요)",
                "#D96B6B",
                artResourcePath: "PrototypeArt/Story/IntroCut03"),
            new StoryIntroCut(
                4,
                "떨어진 선글라스",
                "어서 이 썬구리를 단장님께! 앗! 선글라스가 바닥으로 떨어졌다!",
                "선글라스를 들고 뛰던 참새 이등병이 부딪혀 넘어지고, 선글라스가 아래로 떨어지는 장면. (아트 필요)",
                "#B9D4E8",
                artResourcePath: "PrototypeArt/Story/IntroCut04"),
            new StoryIntroCut(
                5,
                "어둠 속의 고양이",
                "어둠 속에서 빛나는 눈. 고양이가 외친다. 받아라, 부풀린 꼬리 채찍!",
                "떨어진 선글라스 옆에서 겁에 질린 참새 이등병, 어둠 속 고양이의 빛나는 눈과 단장님의 뒷모습. (아트 필요)",
                "#FFE082",
                artResourcePath: "PrototypeArt/Story/IntroCut05"),
            new StoryIntroCut(
                6,
                "까마귀 연구소",
                "눈을 뜬 곳은 까마귀 연구소. 단장님은 쓰러졌고, 나는 한쪽 날개를 다쳐 더는 날 수 없었다.",
                "까마귀 연구소 병상에서 깨어난 참새 이등병과, 옆에 놓인 붕대와 깨진 선글라스. (아트 필요)",
                "#7AE0D6",
                artResourcePath: "PrototypeArt/Story/IntroCut06"),
            new StoryIntroCut(
                7,
                "전봇대 전용 무기",
                "까마귀들은 전력을 모으는 무기를 건넸다. 너무 강해 날아다니는 조류는 쓰기 힘들지만, 다친 나는 버틸 수 있다!",
                "참새 이등병이 전봇대 전용 무기 앞에서 전력을 충전하고, 동료 새들의 스킬 게이지가 빛나는 장면. (아트 필요)",
                "#9BE36D",
                artResourcePath: "PrototypeArt/Story/IntroCut07")
        };

    public static IReadOnlyList<StoryIntroCut> GetCuts()
    {
        return Cuts;
    }
}
