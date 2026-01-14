using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillEditor.Editor
{
    public class CommentNode : BaseNode
    {
        public string Comment;
        private TextField _commentField;

        public CommentNode(SkillGraphView graphView) 
            : base(graphView, "📝 Comment", new Color(0.3f, 0.3f, 0.3f))
        {
            _commentField = new TextField()
            {
                multiline = true,
                value = ""
            };
            _commentField.style.minWidth = 200;
            _commentField.style.minHeight = 60;
            _commentField.RegisterValueChangedCallback(evt => Comment = evt.newValue);
            
            mainContainer.Add(_commentField);
            
            RefreshExpandedState();
            RefreshPorts();
        }
        
        public void SetComment(string comment)
        {
            Comment = comment;
            _commentField.value = comment;
        }
    }
}
