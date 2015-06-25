﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio;

namespace MarketingNotifications.Models
{
    public class Subscriber
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public bool Subscribed { get; set; }

        public void SendMessage(string message, string imageUrl)
        {
            var twilioNumber = ConfigurationManager.AppSettings["TwilioPhoneNumber"];
            var client = new TwilioRestClient(
                ConfigurationManager.AppSettings["TwilioAccountSid"],
                ConfigurationManager.AppSettings["TwilioAuthToken"]
            );
                
            if(imageUrl != "")
            {
                client.SendMessage(twilioNumber, this.PhoneNumber, message, new string[] { imageUrl });    
            }
            else
            {
                client.SendMessage(twilioNumber, this.PhoneNumber, message);
            }
             
        }
    }

    public interface ISubscriberContext
    {
        DbSet<Subscriber> Subscribers { get;  }
        int SaveChanges();
    }

    public class SubscriberContext : DbContext, ISubscriberContext
    {
        public DbSet<Subscriber> Subscribers { get; set; }
    }

    public class TestSubscriberContext : ISubscriberContext
    {
        public DbSet<Subscriber> Subscribers { get; set; }

        public TestSubscriberContext()
        {
            this.Subscribers = new TestDbSet<Subscriber>();
        }
        public int SaveChangesCount { get; private set; }
        public int SaveChanges()
        {
            this.SaveChangesCount++;
            return 1;
        } 
    }
}
