using UnityEngine;
using System.Collections;

namespace Battlehub.RTEditor
{
    public class Expander : MonoBehaviour
    {
        public GameObject Expanded;
        public GameObject Collapsed;
        public GameObject EditorPanel;

        public bool m_isExpanded;
        public bool IsExpanded
        {
            get { return m_isExpanded; }
            set
            {
                m_isExpanded = value;
                Expanded.SetActive(m_isExpanded);
                Collapsed.SetActive(!m_isExpanded);
            }
        }

        void Start()
        {
            if(!IsExpanded)
            {
                if(EditorPanel) EditorPanel.gameObject.SetActive(m_isExpanded);
                Expanded.SetActive(m_isExpanded);
                Collapsed.SetActive(!m_isExpanded);
            }else
            {
                if (EditorPanel) EditorPanel.gameObject.SetActive(m_isExpanded);
                Expanded.SetActive(m_isExpanded);
                Collapsed.SetActive(!m_isExpanded);
            }
        }

        public void ShowPanel(bool value)
        {
            EditorPanel.gameObject.SetActive(value);
            Expanded.SetActive(value);
            Collapsed.SetActive(!value);
        }
    }

}
