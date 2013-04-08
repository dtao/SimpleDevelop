using System.Reflection;

namespace SimpleDevelop.CodeCompletion
{
    class EventCompletionData : CompletionData<EventInfo>
    {
        public EventCompletionData(EventInfo eventInfo) : base(eventInfo)
        {
            Image = "Event";
        }

        public override int Rank
        {
            get { return 3; }
        }
        
        public override object Description
        {
            get { return GetFriendlyTypeName(_memberInfo.EventHandlerType.Name); }
        }
    }
}
