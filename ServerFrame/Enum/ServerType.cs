
namespace ServerFrame
{
    /**
     * ServerType 枚举值变量命名规则 = 项目导出的exe名称
     * 如 GateServer.exe 则该枚举值为GateServer 
     * 不可省略为Gate
     */
    public enum ServerType
    {
        Invalid = 0,
        GlobalServer = 1,
        BarrackServer = 2,
        GateServer = 3,
        ManagerServer = 4,
        RelationServer = 5,
        ZoneServer = 6,
        BattleManagerServer = 7,
        BattleServer = 8,
        CrossServer = 9,
        AnalysisServer = 10,
        PayServer = 11,
        //ChatManagerServer = 9

    }
}
