﻿using System;

namespace Puresharp
{
    public class Visitor<T> : IVisitor<T>
        where T : class
    {
        private Action<T> m_Action;

        public Visitor(Action<T> action)
        {
            this.m_Action = action;
        }

        public void Visit(T item)
        {
            this.m_Action(item);
        }
    }
}
