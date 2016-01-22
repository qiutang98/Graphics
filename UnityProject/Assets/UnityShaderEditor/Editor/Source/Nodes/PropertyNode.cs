using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Graphs;
using UnityEngine;

namespace UnityEditor.MaterialGraph
{
    public abstract class PropertyNode : BaseMaterialNode
    {
        [SerializeField]
        private string m_PropertyName;

        [SerializeField]
        private string m_Description;

        [SerializeField]
        private bool m_Exposed;

        public bool exposed
        {
            get { return m_Exposed; }
        }

        public string description
        {
            get
            {
                if (string.IsNullOrEmpty(m_Description))
                    return propertyName;

                return m_Description;
            }
            set { m_Description = value; }
        }

        public virtual string propertyName
        {
            get
            {
                if (!exposed || string.IsNullOrEmpty(m_PropertyName))
                    return string.Format("{0}_{1}_Uniform", name, Math.Abs(GetInstanceID()));

                return m_PropertyName + "_Uniform";
            }
            set { m_PropertyName = value; }
        }

        public abstract PropertyType propertyType { get; }

        public abstract PreviewProperty GetPreviewProperty();
        
        public override string GetOutputVariableNameForSlot(Slot s, GenerationMode generationMode)
        {
            return propertyName;
        }
        
        public override float GetNodeUIHeight(float width)
        {
            return 2 * EditorGUIUtility.singleLineHeight;
        }

        protected override void CollectPreviewMaterialProperties (List<PreviewProperty> properties)
        {
            base.CollectPreviewMaterialProperties(properties);
            properties.Add(GetPreviewProperty());
        }

        private static int fcuckingtest = 0;
        protected override bool CalculateNodeHasError()
        {
            fcuckingtest++;

            if (fcuckingtest == 100)
            {
                Debug.Log("stack");
            }

            if (!exposed)
                return false;

            var allNodes = pixelGraph.nodes;
            foreach (var n in allNodes.OfType<PropertyNode>())
            {
                if (n == this)
                    continue;;

                if (n.propertyName == propertyName)
                {
                    return true;
                }
            }
            return false;
        }

        public override bool OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            m_Exposed = EditorGUILayout.Toggle("Exposed Property", m_Exposed);
            if (m_Exposed)
                m_PropertyName = EditorGUILayout.DelayedTextField("Property Name", m_PropertyName);

            var modified = EditorGUI.EndChangeCheck();
            if (modified)
            {
                var bmg = (graph as BaseMaterialGraph);
                if (bmg == null)
                    return false;

                bmg.RevalidateGraph();
            }

            if (m_Exposed)
                m_Description = EditorGUILayout.TextField("Description", m_Description);
            
            modified |= base.OnGUI();
            return modified;
        }
    }
}
