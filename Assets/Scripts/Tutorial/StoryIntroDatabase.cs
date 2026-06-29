using System.Collections.Generic;

public static class StoryIntroDatabase
{
    public const string PlayerRole = "\uCC38\uC0C8 \uC774\uB4F1\uBCD1";
    public const string EnemyFaction = "\uACE0\uC591\uC774";
    public const string WarObjective = "\uC804\uBD07\uB300";
    public const string VisualStyle =
        "\uD53D\uC140 \uC544\uD2B8, \uADC0\uC5EC\uC6B4 \uB3C4\uD2B8 \uC2A4\uD0C0\uC77C";

    private static readonly List<StoryIntroCut> Cuts =
        new List<StoryIntroCut>
        {
            new StoryIntroCut(
                1,
                "\uD3C9\uD654\uB85C\uC6B4 \uC804\uBD07\uB300",
                "\uC6B0\uB9AC \uB3D9\uB124 \uC870\uB958\uB4E4\uC740 \uC804\uBD07\uB300\uB97C \uC9C0\uD0A4\uBA70 \uC0B4\uACE0 \uC788\uC5C8\uB2E4.",
                "(\uC544\uD2B8 \uD544\uC694) \uD3C9\uD654\uB85C\uC6B4 \uC804\uBD07\uB300\uC640 \uC791\uC740 \uC870\uB958 \uBD80\uB300",
                "#87C7FF",
                artResourcePath: "PrototypeArt/Story/IntroCut01"),
            new StoryIntroCut(
                2,
                "\uCC38\uC0C8 \uC774\uB4F1\uBCD1",
                "\uB098\uB294 \uCC38\uC0C8 \uC774\uB4F1\uBCD1. \uC67C\uCABD \uB0A0\uAC1C\uAC00 \uBD80\uB7EC\uC838 \uC55E\uC5D0\uC11C \uC2F8\uC6B8 \uC218 \uC5C6\uB2E4.",
                "(\uC544\uD2B8 \uD544\uC694) \uB2E4\uCE5C \uB0A0\uAC1C\uB97C \uAC10\uC2FC \uC791\uC740 \uCC38\uC0C8 \uC774\uB4F1\uBCD1",
                "#F5A64A",
                artResourcePath: "PrototypeArt/Story/IntroCut02"),
            new StoryIntroCut(
                3,
                "\uACE0\uC591\uC774 \uC2B5\uACA9",
                "\uACE0\uC591\uC774\uB4E4\uC774 \uC804\uBD07\uB300\uB97C \uCC28\uC9C0\uD558\uB824\uACE0 \uBAB0\uB824\uC654\uB2E4.",
                "(\uC544\uD2B8 \uD544\uC694) \uB9C8\uC744 \uC544\uB798\uC5D0\uC11C \uBAB0\uB824\uC624\uB294 \uADC0\uC5EC\uC6B4 \uACE0\uC591\uC774 \uC801\uB4E4",
                "#D96B6B",
                artResourcePath: "PrototypeArt/Story/IntroCut03"),
            new StoryIntroCut(
                4,
                "\uB5A8\uC5B4\uC9C4 \uBB34\uAE30",
                "\uC804\uD22C \uC911 \uAE4C\uB9C8\uADC0\uB4E4\uC774 \uC4F0\uB358 \uC804\uB825 \uCDA9\uC804 \uBB34\uAE30\uAC00 \uB0B4 \uC55E\uC5D0 \uB5A8\uC5B4\uC84C\uB2E4.",
                "(\uC544\uD2B8 \uD544\uC694) \uC804\uAE30\uAC00 \uD750\uB974\uB294 \uC791\uC740 \uCDA9\uC804 \uBB34\uAE30",
                "#B9D4E8",
                artResourcePath: "PrototypeArt/Story/IntroCut04"),
            new StoryIntroCut(
                5,
                "\uB4A4\uC5D0\uC11C \uB3D5\uB294 \uC5ED\uD560",
                "\uB098\uB294 \uB4A4\uC5D0\uC11C \uC804\uB825\uC744 \uBAA8\uC544 \uB3D9\uB8CC\uB4E4\uC774 \uC2A4\uD0AC\uC744 \uC4F0\uB3C4\uB85D \uB3D5\uAE30\uB85C \uD588\uB2E4.",
                "(\uC544\uD2B8 \uD544\uC694) \uCC38\uC0C8 \uC774\uB4F1\uBCD1\uC774 \uB4A4\uC5D0\uC11C \uC804\uB825\uC744 \uCDA9\uC804\uD558\uB294 \uC7A5\uBA74",
                "#FFE082",
                artResourcePath: "PrototypeArt/Story/IntroCut05"),
            new StoryIntroCut(
                6,
                "\uB3D9\uB8CC \uBAA8\uC9D1",
                "\uC804\uBD07\uB300\uB97C \uC9C0\uD0A4\uB824\uBA74 \uD568\uAED8 \uC2F8\uC6B8 \uB3D9\uB8CC\uAC00 \uD544\uC694\uD558\uB2E4.",
                "(\uC544\uD2B8 \uD544\uC694) \uC5EC\uB7EC \uC870\uB958 \uB3D9\uB8CC\uB4E4\uC774 \uBAA8\uC774\uB294 \uC7A5\uBA74",
                "#7AE0D6",
                artResourcePath: "PrototypeArt/Story/IntroCut06"),
            new StoryIntroCut(
                7,
                "\uC791\uC804 \uC2DC\uC791",
                "\uC774\uC81C \uB3D9\uB8CC\uB4E4\uACFC \uD568\uAED8 \uACE0\uC591\uC774\uC5D0\uAC8C\uC11C \uC804\uBD07\uB300\uB97C \uC9C0\uCF1C\uC57C \uD55C\uB2E4.",
                "(\uC544\uD2B8 \uD544\uC694) \uC804\uBD07\uB300 \uBC29\uC5B4 \uC791\uC804\uC744 \uC2DC\uC791\uD558\uB294 \uC7A5\uBA74",
                "#9BE36D",
                artResourcePath: "PrototypeArt/Story/IntroCut07")
        };

    public static List<StoryIntroCut> GetCuts()
    {
        return Cuts;
    }
}
