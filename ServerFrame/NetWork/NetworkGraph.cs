
using DataProperty;
using System;
using System.Collections.Generic;
namespace ServerFrame
{
    public class JoinStrategyModel
    {
        private ServerType serverType = ServerType.Invalid;
        public ServerType ServerType
        { get { return serverType; } }
        private Dictionary<ServerType, JoinStrategy> strategyList = new Dictionary<ServerType, JoinStrategy>();
        public Dictionary<ServerType, JoinStrategy> StrategyList
        { get { return strategyList; } }

        public JoinStrategyModel(Data data)
        {
            serverType = (ServerType)Enum.Parse(typeof(ServerType), data.Name);
            foreach (ServerType sType in Enum.GetValues(typeof(ServerType)))
            {
                string strategyName = data.GetString(sType.ToString());
                if (!string.IsNullOrEmpty(strategyName))
                {
                    JoinStrategy strategy = (JoinStrategy)Enum.Parse(typeof(JoinStrategy), strategyName);
                    strategyList.Add(sType, strategy);
                    //Logger.Log.Warn("{0} strage {1} -- {2}", serverType, sType, strategy);
                }
            }
        }

        public bool NeedConnect(int sourceMainId, ServerType dest, int destMainId)
        {
            JoinStrategy strategy = JoinStrategy.None;
            if (!strategyList.TryGetValue(dest, out strategy))
            {
                return false;
            }
            switch (strategy)
            { 
                case JoinStrategy.None:
                case JoinStrategy.AcceptAll:
                case JoinStrategy.AcceptById:
                    return false;
                case JoinStrategy.ConnectAll:
                    return true;
                case JoinStrategy.ConnectById:
                    return sourceMainId == destMainId;
                case JoinStrategy.BothById:
                    return sourceMainId < destMainId;
                default:
                    Logger.Log.Error("got invalid join strategy {0}", strategy);
                    return false;
            }
        }

        public bool NeedAccept(int sourceMainId, ServerType dest, int destMainId)
        {
            JoinStrategy strategy = JoinStrategy.None;
            if (!strategyList.TryGetValue(dest, out strategy))
            {
                return false;
            }
            switch (strategy)
            { 
                case JoinStrategy.None:
                case JoinStrategy.ConnectAll:
                case JoinStrategy.ConnectById:
                    return false;
                case JoinStrategy.AcceptAll:
                    return true;
                case JoinStrategy.AcceptById:
                    return sourceMainId == destMainId;
                case JoinStrategy.BothById:
                    return sourceMainId > destMainId;
                default:
                    Logger.Log.Error("got invalid join strategy {0}", strategy);
                    return false;
            }
        }
    }

    public class NetworkGraph
    {
        private static Dictionary<ServerType, JoinStrategyModel> graph = new Dictionary<ServerType, JoinStrategyModel>();
        public static Dictionary<ServerType, JoinStrategyModel> Graph
        { get { return graph; } }

        public static void Init()
        {
            graph.Clear();
            DataList dataLiat = DataListManager.inst.GetDataList("NetworkGraph");
            foreach (var data in dataLiat.AllData.Values)
            {
                JoinStrategyModel model = new JoinStrategyModel(data);
                graph.Add(model.ServerType, model);
            }
        }

        public static bool NeedConnect(ServerType source, int sourceMainId, ServerType dest, int destMainId)
        {
            JoinStrategyModel model = null;
            if (!graph.TryGetValue(source, out model))
            {
                return false;
            }
            return model.NeedConnect(sourceMainId, dest, destMainId);
        }

        public static bool NeedAccept(ServerType source, int sourceMainId, ServerType dest, int destMainId)
        {
            JoinStrategyModel model = null;
            if (!graph.TryGetValue(source, out model))
            {
                return false;
            }
            return model.NeedAccept(sourceMainId, dest, destMainId);
        }

    }
}
