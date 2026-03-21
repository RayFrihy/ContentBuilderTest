using System;
using NUnit.Framework;
using R3;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Conditions.Interaction;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.Tests.Conditions
{
    [TestFixture]
    public class InteractionConditionTests
    {
        private MockEventBus _eventBus;

        [SetUp]
        public void SetUp()
        {
            _eventBus = new MockEventBus();
        }

        [TearDown]
        public void TearDown()
        {
            _eventBus.Dispose();
        }

        #region EventBusCondition

        [Test]
        public void EventBusCondition_NullEventBus_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new EventBusCondition(null, "SomeEvent", "target1"));
        }

        [Test]
        public void EventBusCondition_NullEventName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new EventBusCondition(_eventBus, null, "target1"));
        }

        [Test]
        public void EventBusCondition_NullTargetId_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new EventBusCondition(_eventBus, "SomeEvent", null));
        }

        [Test]
        public void EventBusCondition_Evaluate_PrependsFalse()
        {
            var condition = new EventBusCondition(_eventBus, "ButtonPressed", "btn1");

            bool? lastValue = null;
            var subscription = condition.Evaluate().Subscribe(v => lastValue = v);

            Assert.AreEqual(false, lastValue);

            subscription.Dispose();
        }

        [Test]
        public void EventBusCondition_Evaluate_MatchingEvent_ReturnsTrue()
        {
            var condition = new EventBusCondition(_eventBus, "ButtonPressed", "btn1");

            bool? lastValue = null;
            var subscription = condition.Evaluate().Subscribe(v => lastValue = v);

            _eventBus.Publish("ButtonPressed", "btn1");
            Assert.AreEqual(true, lastValue);

            subscription.Dispose();
        }

        [Test]
        public void EventBusCondition_Evaluate_NonMatchingTargetId_ReturnsFalse()
        {
            var condition = new EventBusCondition(_eventBus, "ButtonPressed", "btn1");

            bool? lastValue = null;
            var subscription = condition.Evaluate().Subscribe(v => lastValue = v);

            _eventBus.Publish("ButtonPressed", "btn2");
            Assert.AreEqual(false, lastValue);

            subscription.Dispose();
        }

        [Test]
        public void EventBusCondition_Evaluate_NonStringPayload_ReturnsFalse()
        {
            var condition = new EventBusCondition(_eventBus, "ButtonPressed", "btn1");

            bool? lastValue = null;
            var subscription = condition.Evaluate().Subscribe(v => lastValue = v);

            _eventBus.Publish("ButtonPressed", 42);
            Assert.AreEqual(false, lastValue);

            subscription.Dispose();
        }

        [Test]
        public void EventBusCondition_Evaluate_DifferentEventName_DoesNotTrigger()
        {
            var condition = new EventBusCondition(_eventBus, "ButtonPressed", "btn1");

            bool? lastValue = null;
            var subscription = condition.Evaluate().Subscribe(v => lastValue = v);

            _eventBus.Publish("ButtonReleased", "btn1");
            Assert.AreEqual(false, lastValue);

            subscription.Dispose();
        }

        #endregion

        #region ObjectHoveredCondition

        [Test]
        public void ObjectHoveredCondition_NullEventBus_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ObjectHoveredCondition(null, "obj1"));
        }

        [Test]
        public void ObjectHoveredCondition_NullTargetObjectId_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ObjectHoveredCondition(_eventBus, null));
        }

        [Test]
        public void ObjectHoveredCondition_Evaluate_PrependsFalse()
        {
            var condition = new ObjectHoveredCondition(_eventBus, "obj1");

            bool? lastValue = null;
            var subscription = condition.Evaluate().Subscribe(v => lastValue = v);

            Assert.AreEqual(false, lastValue);

            subscription.Dispose();
        }

        [Test]
        public void ObjectHoveredCondition_HoverEnter_MatchingTarget_ReturnsTrue()
        {
            var condition = new ObjectHoveredCondition(_eventBus, "obj1");

            bool? lastValue = null;
            var subscription = condition.Evaluate().Subscribe(v => lastValue = v);

            _eventBus.Publish("ObjectHoverEnter", "obj1");
            Assert.AreEqual(true, lastValue);

            subscription.Dispose();
        }

        [Test]
        public void ObjectHoveredCondition_HoverExit_MatchingTarget_ReturnsFalse()
        {
            var condition = new ObjectHoveredCondition(_eventBus, "obj1");

            bool? lastValue = null;
            var subscription = condition.Evaluate().Subscribe(v => lastValue = v);

            _eventBus.Publish("ObjectHoverEnter", "obj1");
            Assert.AreEqual(true, lastValue);

            _eventBus.Publish("ObjectHoverExit", "obj1");
            Assert.AreEqual(false, lastValue);

            subscription.Dispose();
        }

        [Test]
        public void ObjectHoveredCondition_HoverEnter_NonMatchingTarget_DoesNotChange()
        {
            var condition = new ObjectHoveredCondition(_eventBus, "obj1");

            bool? lastValue = null;
            var subscription = condition.Evaluate().Subscribe(v => lastValue = v);

            _eventBus.Publish("ObjectHoverEnter", "obj2");
            Assert.AreEqual(false, lastValue);

            subscription.Dispose();
        }

        [Test]
        public void ObjectHoveredCondition_HoverExit_NonMatchingTarget_DoesNotChange()
        {
            var condition = new ObjectHoveredCondition(_eventBus, "obj1");

            bool? lastValue = null;
            var subscription = condition.Evaluate().Subscribe(v => lastValue = v);

            _eventBus.Publish("ObjectHoverEnter", "obj1");
            Assert.AreEqual(true, lastValue);

            _eventBus.Publish("ObjectHoverExit", "obj2");
            Assert.AreEqual(true, lastValue);

            subscription.Dispose();
        }

        #endregion

        #region ObjectGrabbedCondition

        [Test]
        public void ObjectGrabbedCondition_NullEventBus_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ObjectGrabbedCondition(null, "obj1"));
        }

        [Test]
        public void ObjectGrabbedCondition_NullTargetObjectId_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ObjectGrabbedCondition(_eventBus, null));
        }

        [Test]
        public void ObjectGrabbedCondition_Evaluate_PrependsFalse()
        {
            var condition = new ObjectGrabbedCondition(_eventBus, "obj1");

            bool? lastValue = null;
            var subscription = condition.Evaluate().Subscribe(v => lastValue = v);

            Assert.AreEqual(false, lastValue);

            subscription.Dispose();
        }

        [Test]
        public void ObjectGrabbedCondition_Evaluate_MatchingTarget_ReturnsTrue()
        {
            var condition = new ObjectGrabbedCondition(_eventBus, "obj1");

            bool? lastValue = null;
            var subscription = condition.Evaluate().Subscribe(v => lastValue = v);

            _eventBus.Publish("ObjectGrabbed", "obj1");
            Assert.AreEqual(true, lastValue);

            subscription.Dispose();
        }

        [Test]
        public void ObjectGrabbedCondition_Evaluate_NonMatchingTarget_ReturnsFalse()
        {
            var condition = new ObjectGrabbedCondition(_eventBus, "obj1");

            bool? lastValue = null;
            var subscription = condition.Evaluate().Subscribe(v => lastValue = v);

            _eventBus.Publish("ObjectGrabbed", "obj2");
            Assert.AreEqual(false, lastValue);

            subscription.Dispose();
        }

        [Test]
        public void ObjectGrabbedCondition_Reset_ClearsGrabbedState()
        {
            var condition = new ObjectGrabbedCondition(_eventBus, "obj1");

            condition.Reset();

            // After reset, a fresh Evaluate should prepend false
            bool? lastValue = null;
            var subscription = condition.Evaluate().Subscribe(v => lastValue = v);

            Assert.AreEqual(false, lastValue);

            subscription.Dispose();
        }

        #endregion

        #region GesturePerformedCondition

        [Test]
        public void GesturePerformedCondition_NullEventBus_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new GesturePerformedCondition(null, "obj1", GestureType.Tap));
        }

        [Test]
        public void GesturePerformedCondition_NullTargetObjectId_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new GesturePerformedCondition(_eventBus, null, GestureType.Tap));
        }

        [Test]
        public void GesturePerformedCondition_Evaluate_PrependsFalse()
        {
            var condition = new GesturePerformedCondition(_eventBus, "obj1", GestureType.Swipe);

            bool? lastValue = null;
            var subscription = condition.Evaluate().Subscribe(v => lastValue = v);

            Assert.AreEqual(false, lastValue);

            subscription.Dispose();
        }

        [Test]
        public void GesturePerformedCondition_Evaluate_MatchingGesture_ReturnsTrue()
        {
            var condition = new GesturePerformedCondition(_eventBus, "obj1", GestureType.Pinch);

            bool? lastValue = null;
            var subscription = condition.Evaluate().Subscribe(v => lastValue = v);

            _eventBus.Publish("GesturePerformed", new GestureEventData("obj1", GestureType.Pinch));
            Assert.AreEqual(true, lastValue);

            subscription.Dispose();
        }

        [Test]
        public void GesturePerformedCondition_Evaluate_WrongGestureType_ReturnsFalse()
        {
            var condition = new GesturePerformedCondition(_eventBus, "obj1", GestureType.Tap);

            bool? lastValue = null;
            var subscription = condition.Evaluate().Subscribe(v => lastValue = v);

            _eventBus.Publish("GesturePerformed", new GestureEventData("obj1", GestureType.Swipe));
            Assert.AreEqual(false, lastValue);

            subscription.Dispose();
        }

        [Test]
        public void GesturePerformedCondition_Evaluate_WrongObjectId_ReturnsFalse()
        {
            var condition = new GesturePerformedCondition(_eventBus, "obj1", GestureType.DoubleTap);

            bool? lastValue = null;
            var subscription = condition.Evaluate().Subscribe(v => lastValue = v);

            _eventBus.Publish("GesturePerformed", new GestureEventData("obj2", GestureType.DoubleTap));
            Assert.AreEqual(false, lastValue);

            subscription.Dispose();
        }

        [Test]
        public void GesturePerformedCondition_Evaluate_NonGesturePayload_ReturnsFalse()
        {
            var condition = new GesturePerformedCondition(_eventBus, "obj1", GestureType.LongPress);

            bool? lastValue = null;
            var subscription = condition.Evaluate().Subscribe(v => lastValue = v);

            _eventBus.Publish("GesturePerformed", "not a gesture");
            Assert.AreEqual(false, lastValue);

            subscription.Dispose();
        }

        #endregion
    }
}
