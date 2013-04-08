using System.Reflection;

namespace SimpleDevelop.CodeCompletion
{
    class EventCompletionData : CompletionData<EventInfo>
    {
        public EventCompletionData(EventInfo eventInfo) : base(eventInfo)
        {
            Image = "Event";
        }

        public override object Description
        {
            get { return string.Format("{0} {1}", _memberInfo.EventHandlerType.Name, _memberInfo.Name); }
        }
    }
}
