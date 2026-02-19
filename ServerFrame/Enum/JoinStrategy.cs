
namespace ServerFrame
{
    // cluster之间的互联策略
    // 枚举值名字不可随意修改，与NetworkGraph.xml配对
    public enum JoinStrategy
    {
        None = 0,
        ConnectAll = 1, // 只要是该类型的服务器，全部需要连接 入Gate->Global
        ConnectById = 2, // mainId相同的情况下才去主动连接该类型服务器 1001 1 Zone -> 1001 Manager
        AcceptAll = 3, // 只要是该服务器就可以Accept该类型服务器的连接请求 入 Global <- Gate
        AcceptById = 4, // mainId相同的情况下才去Accept该类型服务器的连接请求 1001 1 Zone -> 1001 Manager 
        BothById = 5, // 即主动连接该类型又接收该类型连接 如 Manager <-> Manager 通过MainId大小决定连接方式
    }
}
