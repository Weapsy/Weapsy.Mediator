﻿using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Weapsy.Cqrs.Commands;
using Weapsy.Cqrs.Domain;
using Weapsy.Cqrs.Events;
using Weapsy.Cqrs.Queries;
using Weapsy.Cqrs.Tests.Fakes;

namespace Weapsy.Cqrs.Tests
{
    [TestFixture]
    public class DispatcherTests
    {
        private IDispatcher _sut;

        private Mock<ICommandSenderAsync> _commandSenderAsync;
        private Mock<ICommandSender> _commandSender;

        private Mock<IEventPublisherAsync> _eventPublisherAsync;
        private Mock<IEventPublisher> _eventPublisher;

        private Mock<IQueryProcessorAsync> _queryDispatcherAsync;
        private Mock<IQueryProcessor> _queryDispatcher;

        private CreateSomething _createSomething;
        private SomethingCreated _somethingCreated;
        private GetSomething _getSomething;
        private Something _something;
        private CreateAggregate _createAggregate;

        [SetUp]
        public void SetUp()
        {
            _createSomething = new CreateSomething();
            _somethingCreated = new SomethingCreated();
            _getSomething = new GetSomething();
            _something = new Something();
            _createAggregate = new CreateAggregate();

            _commandSenderAsync = new Mock<ICommandSenderAsync>();
            _commandSenderAsync
                .Setup(x => x.SendAsync(_createSomething))
                .Returns(Task.CompletedTask);
            _commandSenderAsync
                .Setup(x => x.SendAndPublishAsync(_createSomething))
                .Returns(Task.CompletedTask);
            _commandSenderAsync
                .Setup(x => x.SendAndPublishAsync<ICommand>(_createSomething))
                .Returns(Task.CompletedTask);
            _commandSenderAsync
                .Setup(x => x.SendAndPublishAsync<IDomainCommand, IAggregateRoot>(_createAggregate))
                .Returns(Task.CompletedTask);

            _commandSender = new Mock<ICommandSender>();
            _commandSender
                .Setup(x => x.Send(_createSomething));
            _commandSender
                .Setup(x => x.SendAndPublish(_createSomething));
            _commandSender
                .Setup(x => x.SendAndPublish<IDomainCommand, IAggregateRoot>(_createAggregate));

            _eventPublisherAsync = new Mock<IEventPublisherAsync>();
            _eventPublisherAsync
                .Setup(x => x.PublishAsync(_somethingCreated))
                .Returns(Task.CompletedTask);

            _eventPublisher = new Mock<IEventPublisher>();
            _eventPublisher
                .Setup(x => x.Publish(_somethingCreated));

            _queryDispatcherAsync = new Mock<IQueryProcessorAsync>();
            _queryDispatcherAsync
                .Setup(x => x.ProcessAsync<IQuery, Something>(_getSomething))
                .ReturnsAsync(_something);

            _queryDispatcher = new Mock<IQueryProcessor>();
            _queryDispatcher
                .Setup(x => x.Process<IQuery, Something>(_getSomething))
                .Returns(_something);

            _sut = new Dispatcher(_commandSenderAsync.Object, 
                _commandSender.Object, 
                _eventPublisherAsync.Object,
                _eventPublisher.Object,
                _queryDispatcherAsync.Object,
                _queryDispatcher.Object);
        }

        [Test]
        public async Task SendsCommandAsync()
        {
            await _sut.SendAsync(_createSomething);
            _commandSenderAsync.Verify(x => x.SendAsync(_createSomething), Times.Once);
        }

        [Test]
        public async Task SendsCommandAndPublishAsync()
        {
            await _sut.SendAndPublishAsync(_createSomething);
            _commandSenderAsync.Verify(x => x.SendAndPublishAsync(_createSomething), Times.Once);
        }

        [Test]
        public async Task SendsCommandAndPublishWithAggregateAsync()
        {
            await _sut.SendAndPublishAsync<IDomainCommand, IAggregateRoot>(_createAggregate);
            _commandSenderAsync.Verify(x => x.SendAndPublishAsync<IDomainCommand, IAggregateRoot>(_createAggregate), Times.Once);
        }

        [Test]
        public void SendsCommand()
        {
            _sut.Send(_createSomething);
            _commandSender.Verify(x => x.Send(_createSomething), Times.Once);
        }

        [Test]
        public void SendsCommandAndPublish()
        {
            _sut.SendAndPublish(_createSomething);
            _commandSender.Verify(x => x.SendAndPublish(_createSomething), Times.Once);
        }

        [Test]
        public void SendsCommandAndPublishWithAggregate()
        {
            _sut.SendAndPublish<IDomainCommand, IAggregateRoot>(_createAggregate);
            _commandSender.Verify(x => x.SendAndPublish<IDomainCommand, IAggregateRoot>(_createAggregate), Times.Once);
        }

        [Test]
        public async Task PublishEventAsync()
        {
            await _sut.PublishAsync(_somethingCreated);
            _eventPublisherAsync.Verify(x => x.PublishAsync(_somethingCreated), Times.Once);
        }

        [Test]
        public void PublishEvent()
        {
            _sut.Publish(_somethingCreated);
            _eventPublisher.Verify(x => x.Publish(_somethingCreated), Times.Once);
        }

        [Test]
        public async Task GetsResultAsync()
        {
            await _sut.GetResultAsync<GetSomething, Something>(_getSomething);
            _queryDispatcherAsync.Verify(x => x.ProcessAsync<GetSomething, Something>(_getSomething), Times.Once);
        }

        [Test]
        public void GetsResult()
        {
            _sut.GetResult<GetSomething, Something>(_getSomething);
            _queryDispatcher.Verify(x => x.Process<GetSomething, Something>(_getSomething), Times.Once);
        }
    }
}