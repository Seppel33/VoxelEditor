namespace EmailSender
{
    public interface IRichMessageEmailSender : IEmailSender {
        /// <summary>
        /// Sends a message. 
        /// </summary>
        /// <param name="message">Message instance</param>
        void Send(Message message);

        /// <summary>
        /// Sends multiple messages. 
        /// </summary>
        /// <param name="messages">Array of messages</param>
        void Send(Message[] messages);
    }
}