using UnityEngine;

namespace BehaviorTree
{
    // Базовий клас для дерева поведінки
    public abstract class BTree: MonoBehaviour
    {
        protected Node _root = null; // Кореневий вузол дерева

        // Ініціалізація дерева
        protected void Start()
        {
            _root = SetupTree();
        }

        // Оновлення дерева кожен кадр
        protected void Update()
        {
            if (_root != null)
                _root.Evaluate();
        }

        // Метод для налаштування дерева
        protected abstract Node SetupTree();
    }
}