using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketingNotifications.Models
{
    public class Subscriber
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public bool Subscribed { get; set; }
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
