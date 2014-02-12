﻿using Pigeon.Actor;
using Pigeon.Configuration;
using Pigeon.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pigeon.Remote
{
    public class RemoteActorRefProvider : ActorRefProvider
    {
        public RemoteActorRefProvider(ActorSystem system)
            : base(system)
        {
            this.config = system.Settings.Config.GetConfig("akka.remote");
        }

        private Config config;

        public override void Init()
        {
            base.Init();

            var host = config.GetString("server.host");
            var port = config.GetInt("server.port");
            System.ActorRefFactory = (cell, actorPath) => new RemoteActorRef(cell, actorPath, port);
            System.Address = new Address("akka.tcp", System.Name, host, port);
            RemoteHost.StartHost(System, port);
        }

        public override LocalActorRef ActorOf(ActorCell parentContext, Props props, string name)
        {
            var mailbox = (Mailbox)Activator.CreateInstance(props.MailboxType);

            if (props.RouterConfig != null)
            {
                var cell = new RoutedActorCell(parentContext, props, name, mailbox);
                parentContext.NewActor(cell);
                parentContext.Watch(cell.Self);
                return cell.Self;
            }
            else
            {
                var cell = new ActorCell(parentContext, props, name, mailbox);
                parentContext.NewActor(cell);
                parentContext.Watch(cell.Self);
                return cell.Self;
            }
        }
    }
}
