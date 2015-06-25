using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twilio;
using Twilio.TwiML;
using Twilio.TwiML.Mvc;
using MarketingNotifications.Models;
using System.Threading.Tasks;
using MarketingNotifications.Helpers;

namespace MarketingNotifications.Controllers
{
    public class NotificationsController : TwilioController
    {
        ISubscriberContext _db;
        public NotificationsController()
        {
            _db = new SubscriberContext();
        }

        public NotificationsController(ISubscriberContext db)
        {
            _db = db;
        }

        // GET: Notifications
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string message, string imageUrl)
        {
            return View();
        }
        // POST: Incoming
        [HttpPost]
        public TwiMLResult Incoming(string from, string body)
        {
                var isNewSubscriber = false;

                var subscriber = _db.Subscribers.FirstOrDefault(s => s.PhoneNumber == from);
            
                if (subscriber == null)
                {
                    isNewSubscriber = true;
                    subscriber = _db.Subscribers.Add(new Subscriber { PhoneNumber = from });
                }

                body = body.ToLower();

                string output;
                if (isNewSubscriber)
                {
                    output = "Thanks for contacting TWBC! Text 'subscribe' if you would to receive updates via text message.";
                }
                else
                {
                    output = ProcessMessage(body, subscriber);
                }

                // Save any changes made to the Subscriber to the database
                _db.SaveChanges();

                // Generate and return TwiML response
                return GenerateResponse(output);
        }

        private string ProcessMessage(string message, Subscriber subscriber)
        {
            string output;

            if (message == "subscribe" || message == "stahp")
            {
                subscriber.Subscribed = message == "subscribe";
                if (subscriber.Subscribed)
                {
                    output = "You are now subscribed for updates.";
                }
                else
                {
                    output = "You have unsubscribed from notifications. Text 'subscribe' to start receiving updates again.";
                }
            }
            else
            {
                output = "Sorry, we don't recognize that command. Available commands are: 'subscribe' or 'stahp'.";
            }

            return output;
        }

        // Generate TwilioResponse for testability
        private TwiMLResult GenerateResponse(string message)
        {
            var response = new TwilioResponse();
            response.Message(message);
            return TwiML(response);
        }

        private void AddAlert(string alertStyle, string message, bool dismissable)
        {
            var alerts = TempData.ContainsKey(Alert.TempDataKey)
                ? (List<Alert>)TempData[Alert.TempDataKey]
                : new List<Alert>();

            alerts.Add(new Alert
            {
                AlertStyle = alertStyle,
                Message = message,
                Dismissable = dismissable
            });

            TempData[Alert.TempDataKey] = alerts;
        }
    }
}