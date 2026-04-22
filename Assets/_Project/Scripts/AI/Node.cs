using System.Collections.Generic;

namespace BehaviorTree
{
    public enum NodeState
    {
        RUNNING,
        SUCCESS,
        FAILURE
    }

    // Базовий клас для всіх вузлів дерева поведінки
    public class Node
    {
        protected NodeState state;                          // Поточний стан вузла
        public Node parent;                                 // Посилання на батьківський вузол
        protected List<Node> children = new List<Node>();   // Список дочірніх вузлів

        // Конструктор: додаємо дітей
        public Node() { parent = null; }
        public Node(List<Node> children) { Attach(children); }

        public void Attach(List<Node> children)
        {
            foreach (Node child in children)
            {
                child.parent = this;
                this.children.Add(child);
            }
        }

        public virtual NodeState Evaluate() => NodeState.FAILURE;

        // Зберігання даних
        private Dictionary<string, object> _dataContext = new Dictionary<string, object>();

        public void SetData(string key, object value)
        {
            _dataContext[key] = value;
        }

        public object GetData(string key)
        {
            object value = null;
            if (_dataContext.TryGetValue(key, out value)) return value;

            // Якщо тут немає, шукаємо у батька
            Node node = parent;
            while (node != null)
            {
                value = node.GetData(key);
                if (value != null) return value;
                node = node.parent;
            }
            return null;
        }

        public bool ClearData(string key)
        {
            if (_dataContext.ContainsKey(key))
            {
                _dataContext.Remove(key);
                return true;
            }
            Node node = parent;
            while (node != null)
            {
                bool cleared = node.ClearData(key);
                if (cleared) return true;
                node = node.parent;
            }
            return false;
        }
    }
}