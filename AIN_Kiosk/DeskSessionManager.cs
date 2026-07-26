using System;
using System.Collections.Concurrent;

namespace AIN_Kiosk
{
    public sealed class DeskSessionManager
    {
        // قاموس آمن برمجياً لعزل الجلسات المتزامنة بناءً على الـ Guid الخاص بكل جلسة زائر
        private readonly ConcurrentDictionary<Guid, VisitorSession> _sessions = new();

        // بدء جلسة مستقلة جديدة
        public Guid StartSession(string operatorUserId)
        {
            var sessionId = Guid.NewGuid();
            _sessions[sessionId] = new VisitorSession(sessionId, operatorUserId);
            return sessionId;
        }

        // جلب سياق جلسة معينة عند تبديل موظف الاستقبال بين الشاشات
        public VisitorSession Get(Guid sessionId)
        {
            return _sessions.TryGetValue(sessionId, out var s)
                ? s
                : throw new InvalidOperationException("Session not found");
        }

        // إنهاء الجلسة وحذفها مع تطهير بياناتها الحساسة فوراً من الـ RAM
        public void EndSession(Guid sessionId)
        {
            if (_sessions.TryRemove(sessionId, out var s))
            {
                s.WipeSensitiveDataInMemory();
            }
        }
    }
}