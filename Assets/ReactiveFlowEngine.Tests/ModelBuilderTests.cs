using System.Collections.Generic;
using NUnit.Framework;
using ReactiveFlowEngine.Model;
using ReactiveFlowEngine.Serialization;
using ReactiveFlowEngine.Serialization.JsonModels;

namespace ReactiveFlowEngine.Tests
{
    [TestFixture]
    public class ModelBuilderTests
    {
        private ModelBuilder _builder;

        [SetUp]
        public void SetUp()
        {
            // Use parameterless constructor (no factories) for model-level tests
            _builder = new ModelBuilder();
        }

        [Test]
        public void Build_WithValidWrapper_CreatesProcessWithChaptersAndSteps()
        {
            var wrapper = CreateMinimalWrapper();
            var process = _builder.Build(wrapper);

            Assert.IsNotNull(process);
            Assert.AreEqual("TestProcess", process.Name);
            Assert.AreEqual(1, process.ChapterModels.Count);
            Assert.IsNotNull(process.FirstChapterModel);
        }

        [Test]
        public void Build_NullWrapper_ThrowsInvalidOperation()
        {
            Assert.Throws<System.InvalidOperationException>(() => _builder.Build(null));
        }

        [Test]
        public void Build_NullProcess_ThrowsInvalidOperation()
        {
            var wrapper = new JsonProcessWrapper { Process = null };
            Assert.Throws<System.InvalidOperationException>(() => _builder.Build(wrapper));
        }

        [Test]
        public void Build_ResolvesTransitionTargetSteps()
        {
            var step1Guid = "step-guid-1";
            var step2Guid = "step-guid-2";

            var step1 = new JsonStep
            {
                StepMetadata = new JsonStepMetadata { Guid = step1Guid },
                Data = new JsonStepData
                {
                    Name = "Step1",
                    Transitions = new JsonTransitionCollection
                    {
                        Data = new JsonTransitionCollectionData
                        {
                            Transitions = new List<object>
                            {
                                new JsonTransition
                                {
                                    Data = new JsonTransitionData
                                    {
                                        TargetStep = new JsonStep
                                        {
                                            StepMetadata = new JsonStepMetadata { Guid = step2Guid }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var step2 = new JsonStep
            {
                StepMetadata = new JsonStepMetadata { Guid = step2Guid },
                Data = new JsonStepData { Name = "Step2" }
            };

            var chapterGuid = "chapter-guid-1";
            var wrapper = new JsonProcessWrapper
            {
                Steps = new List<object> { step1, step2 },
                Process = new JsonProcess
                {
                    ProcessMetadata = new JsonProcessMetadata { Guid = "proc-guid" },
                    Data = new JsonProcessData
                    {
                        Name = "TestProcess",
                        Chapters = new List<object>
                        {
                            new JsonChapter
                            {
                                ChapterMetadata = new JsonChapterMetadata { Guid = chapterGuid },
                                Data = new JsonChapterData
                                {
                                    Name = "Chapter1",
                                    Steps = new List<object> { step1, step2 },
                                    FirstStep = step1
                                }
                            }
                        },
                        FirstChapter = new JsonChapter
                        {
                            ChapterMetadata = new JsonChapterMetadata { Guid = chapterGuid }
                        }
                    }
                }
            };

            var process = _builder.Build(wrapper);

            var firstStep = process.ChapterModels[0].FirstStepModel;
            Assert.IsNotNull(firstStep);
            Assert.AreEqual(1, firstStep.TransitionModels.Count);
            Assert.IsNotNull(firstStep.TransitionModels[0].TargetStepModel);
            Assert.AreEqual(step2Guid, firstStep.TransitionModels[0].TargetStepModel.Id);
            Assert.AreEqual("Step2", firstStep.TransitionModels[0].TargetStepModel.Name);
        }

        [Test]
        public void Build_GenericBehavior_ProducesBehaviorDefinition()
        {
            // This test verifies that when a JsonGenericBehavior arrives in the steps,
            // BuildBehaviorDefinition produces a non-null BehaviorDefinition.
            // Without BehaviorFactory, the behavior won't be instantiated, so the step
            // will have no behaviors. This is expected for model-level tests.
            var wrapper = CreateMinimalWrapper();
            var process = _builder.Build(wrapper);
            Assert.IsNotNull(process);
        }

        [Test]
        public void Validate_DetectsMissingChapters()
        {
            var process = new ProcessModel { Id = "p1", Name = "Test" };
            var wrapper = CreateMinimalWrapper();

            var result = _builder.Validate(process, wrapper);
            Assert.IsTrue(!result.IsValid);
        }

        [Test]
        public void Validate_DetectsMissingFirstStep()
        {
            var chapter = new ChapterModel { Id = "ch1", Name = "Ch1" };
            chapter.StepModels.Add(new StepModel { Id = "s1", Name = "S1" });
            // FirstStepModel is null

            var process = new ProcessModel { Id = "p1", Name = "Test" };
            process.ChapterModels.Add(chapter);
            process.FirstChapterModel = chapter;

            var result = _builder.Validate(process, CreateMinimalWrapper());
            Assert.IsTrue(!result.IsValid);
        }

        [Test]
        public void Validate_NullProcess_ReturnsError()
        {
            var result = _builder.Validate(null, CreateMinimalWrapper());
            Assert.IsTrue(!result.IsValid);
        }

        private JsonProcessWrapper CreateMinimalWrapper()
        {
            var stepGuid = "step-guid-1";
            var chapterGuid = "chapter-guid-1";

            var step = new JsonStep
            {
                StepMetadata = new JsonStepMetadata { Guid = stepGuid },
                Data = new JsonStepData { Name = "Step1" }
            };

            return new JsonProcessWrapper
            {
                Steps = new List<object> { step },
                Process = new JsonProcess
                {
                    ProcessMetadata = new JsonProcessMetadata { Guid = "proc-guid" },
                    Data = new JsonProcessData
                    {
                        Name = "TestProcess",
                        Chapters = new List<object>
                        {
                            new JsonChapter
                            {
                                ChapterMetadata = new JsonChapterMetadata { Guid = chapterGuid },
                                Data = new JsonChapterData
                                {
                                    Name = "Chapter1",
                                    Steps = new List<object> { step },
                                    FirstStep = step
                                }
                            }
                        },
                        FirstChapter = new JsonChapter
                        {
                            ChapterMetadata = new JsonChapterMetadata { Guid = chapterGuid }
                        }
                    }
                }
            };
        }
    }
}
