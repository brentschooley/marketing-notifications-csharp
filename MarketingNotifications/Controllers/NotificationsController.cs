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

        // POST: Notifications
        [HttpPost]
        public ActionResult Index(string message, string imageUrl)
        {
            try
            {
                foreach (var subscriber in _db.Subscribers.Where(s => s.Subscribed))
                {
                    subscriber.SendMessage(message, imageUrl);
                }

                SuccessAlert("Messages are on their way!");
            }
            catch
            {
                ErrorAlert("Something went wrong. Try again.");
            }
            
            return View();
        }

        // POST: Incoming
        [HttpPost]
        public TwiMLResult Incoming(string from, string body)
        {
            string response;
            Subscriber subscriber = _db.Subscribers.FirstOrDefault(s => s.PhoneNumber == from);

            if (subscriber == null)
            {
                // If subscriber not found, create new subscriber and return signup prompt.
                subscriber = _db.Subscribers.Add(new Subscriber { PhoneNumber = from });
                response = "Thanks for contacting us! Text 'subscribe' if you would like to receive updates via text message.";
            }
            else
            {
                // Otherwise, process the message for existing subscriber.
                response = ProcessMessage(body.ToLower(), subscriber);
            }

            // Save any changes made to the Subscriber to the database
            _db.SaveChanges();

            // Generate and return TwiML response
            return GenerateTwiML(response);
        }

        private string ProcessMessage(string message, Subscriber subscriber)
        {
            string output;

            if (message == "subscribe" || message == "stahp")
            {
                subscriber.Subscribed = message == "subscribe";
                if (subscriber.Subscribed)
                {
                    output = "You are now subscribed for updates. Text 'stahp' at any time to stop receiving updates.";
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

        private TwiMLResult GenerateTwiML(string message)
        {
            var response = new TwilioResponse();
            response.Message(message);
            return TwiML(response);
        }

        #region Alerts
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

        public void SuccessAlert(string message, bool dismissable = false)
        {
            AddAlert(AlertStyles.Success, message, dismissable);
        }

        public void ErrorAlert(string message, bool dismissable = false)
        {
            AddAlert(AlertStyles.Danger, message, dismissable);
        }
        #endregion
    }
}