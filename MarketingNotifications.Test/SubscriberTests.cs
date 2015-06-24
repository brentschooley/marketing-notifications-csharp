using System;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MarketingNotifications.Controllers;
using MarketingNotifications.Models;
using Twilio.TwiML.Mvc;
using System.Linq;

namespace MarketingNotifications.Test
{
    [TestClass]
    public class SubscriberTest
    {
        TestSubscriberContext _db;
        NotificationsController controller;

        [TestInitialize]
        public void Initialize()
        {
            _db = new TestSubscriberContext();
            controller = new NotificationsController(_db);
        }

        [TestMethod]
        public void Subscriber_Created_On_Initial_Message()
        {
            TwiMLResult result = controller.Incoming("+15555551234", "Hi");
            var subscriber = _db.Subscribers.SingleOrDefault(s => s.PhoneNumber == "+15555551234");
            Assert.IsNotNull(subscriber);
            Assert.IsFalse(subscriber.Subscribed);
        }

        [TestMethod]
        public void Subscriber_Subscribed_On_Subscribe_Keyword()
        {
            controller.Incoming("+15555551234", "Hello");
            controller.Incoming("+15555551234", "subscribe");
            var subscriber = _db.Subscribers.SingleOrDefault(s => s.PhoneNumber == "+15555551234");
            Assert.IsNotNull(subscriber);
            Assert.IsTrue(subscriber.Subscribed);
        }

        [TestMethod]
        public void Subscriber_Unsubscribed_On_Unsubscribe_Keyword()
        {
            // Subscribe the user
            controller.Incoming("+15555551234", "Hello");
            controller.Incoming("+15555551234", "subscribe");
            var subscriber = _db.Subscribers.SingleOrDefault(s => s.PhoneNumber == "+15555551234");
            Assert.IsNotNull(subscriber);
            
            // Verify user is subscribed
            Assert.IsTrue(subscriber.Subscribed);
            
            // Send unsubscribe and verify successful unsubscribe
            controller.Incoming("+15555551234", "unsubscribe");
            subscriber = _db.Subscribers.SingleOrDefault(s => s.PhoneNumber == "+15555551234");
            Assert.IsTrue(subscriber.Subscribed);
        }
    }
}
