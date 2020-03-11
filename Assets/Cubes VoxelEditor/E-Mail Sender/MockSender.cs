
using System;

namespace EmailSender
{
    public class MockSender : IRichMessageEmailSender {
        
        public virtual void Send(String from, String to, String subject, String messageText) {

        }

        public virtual void Send(Message message) {

        }

        public virtual void Send(Message[] messages) {

        }
    }
}