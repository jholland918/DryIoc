﻿using System.ComponentModel.Composition;
using System.Linq;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using DryIocAttributes;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class ExportAsDecoratorTests
    {
        [Test]
        public void Decorator_can_be_applied_based_on_Name()
        {
            var container = new Container();
            container.RegisterExports(typeof(LoggingHandlerDecorator), typeof(FastHandler), typeof(SlowHandler));

            Assert.That(container.Resolve<IHandler>("fast"), Is.InstanceOf<FastHandler>());

            var slow = container.Resolve<IHandler>("slow");
            Assert.That(slow, Is.InstanceOf<LoggingHandlerDecorator>());
            Assert.That(((LoggingHandlerDecorator)slow).Handler, Is.InstanceOf<SlowHandler>());
        }

        [Test]
        public void Decorator_can_be_applied_based_on_both_name_and_Metadata()
        {
            var container = new Container();
            container.RegisterExports(typeof(TransactHandlerDecorator), typeof(FastHandler), typeof(SlowHandler),
                typeof(TransactHandler));

            Assert.That(container.Resolve<IHandler>("slow"), Is.InstanceOf<SlowHandler>());
            Assert.That(container.Resolve<IHandler>("fast"), Is.InstanceOf<FastHandler>());

            var transact = container.Resolve<IHandler>("transact");
            Assert.That(transact, Is.InstanceOf<TransactHandlerDecorator>());
            Assert.That(((TransactHandlerDecorator)transact).Handler, Is.InstanceOf<TransactHandler>());
        }

        [Test]
        public void Decorator_can_be_applied_based_on_custom_condition()
        {
            var container = new Container();
            container.RegisterExports(typeof(CustomHandlerDecorator), typeof(FastHandler), typeof(SlowHandler));

            Assert.That(container.Resolve<IHandler>("fast"), Is.InstanceOf<FastHandler>());

            var fast = container.Resolve<IHandler>("slow");
            Assert.That(fast, Is.InstanceOf<CustomHandlerDecorator>());
            Assert.That(((CustomHandlerDecorator)fast).Handler, Is.InstanceOf<SlowHandler>());
        }

        [Test]
        public void Decorator_will_ignore_Import_attribute_on_decorated_service_constructor()
        {
            var container = new Container();
            container.RegisterExports(typeof(FastHandler), typeof(SlowHandler), typeof(DecoratorWithFastHandlerImport));

            var slow = container.Resolve<IHandler>("slow");

            Assert.That(slow, Is.InstanceOf<DecoratorWithFastHandlerImport>());
            Assert.That(((DecoratorWithFastHandlerImport)slow).Handler, Is.InstanceOf<SlowHandler>());
        }

        [Test]
        public void Decorator_supports_matching_by_service_key()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(FoohHandler), typeof(BlahHandler), typeof(FoohDecorator));

            var handler = container.Resolve<IHandler>(BlahFooh.Fooh);

            Assert.That(handler, Is.InstanceOf<FoohDecorator>());
            Assert.That(((FoohDecorator)handler).Handler, Is.InstanceOf<FoohHandler>());
        }

        [Test]
        public void Decorator_may_be_applied_to_Func_decorated()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(DecoratedResult), typeof(FuncDecorator));

            var me = container.Resolve<IDecoratedResult>();
            var result = me.GetResult();

            Assert.AreEqual(2, result);
        }

        [Test]
        public void Only_single_resolution_should_present_for_decorated_service()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(DecoratedResult), typeof(FuncDecorator));

            var registrations = container.GetServiceRegistrations()
                .Where(r => r.ServiceType == typeof(IDecoratedResult))
                .ToArray();

            Assert.AreEqual(1, registrations.Length);
        }

        [Test, Explicit("Related to #141: Support Decorators with open-generic factory methods of T")]
        public void Can_export_decorator_of_T()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(Bee), typeof(D));

            container.Resolve<IBug>();
        }

        internal interface IBug { }
        
        [Export(typeof(IBug))]
        internal class Bee : IBug
        {
        }

        [Export, AsFactory]
        internal static class D
        {
            [Export(typeof(IBug)), AsDecorator]
            public static TBug Decorate<TBug>(TBug bug) where TBug : IBug
            {
                return bug;
            }
        }
    }
}


