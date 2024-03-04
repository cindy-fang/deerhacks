using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
// using SpeechLib;

namespace Unity.VRTemplate
{
    [System.Serializable]
    class Question
    {
        public string questionText;
        public List<string> answers;
        public int correctAnswerIndex; // Index of the correct answer

        public Question(string questionText, List<string> answers, int correctAnswerIndex)
        {
            this.questionText = questionText;
            this.answers = answers;
            this.correctAnswerIndex = correctAnswerIndex;
        }
    }
    /// <summary>
    /// Controls the steps in the in coaching card.
    /// </summary>
    public class StepManager : MonoBehaviour
    {
        // SpVoice voice = new SpVoice();
        [Serializable]
        class Step
        {
            [SerializeField]
            public GameObject stepObject;

            [SerializeField]
            public string buttonText;

            [SerializeField]
            public TMPro.TMP_Dropdown dropdownObject;

            [SerializeField]
            public TMPro.TMP_Text textObject;
        }
        [SerializeField]
        public TextMeshProUGUI answerResultText;
        [SerializeField]
        public TextMeshProUGUI transcriptText;

        [SerializeField]
        public TextMeshProUGUI m_StepButtonTextField;

        [SerializeField]
        Transform controlledObjectTransform;
        [SerializeField]
        GameObject jumpscareObject;

        public AudioSource audioSource;
        public AudioSource bgmAudioSource;
        public AudioSource threatAudioSource;

        [SerializeField]
        private List<AudioClip> correctAnswerAudio;
        [SerializeField]
        private List<AudioClip> incorrectAnswerAudio;


        [SerializeField]
        private AudioClip correctAnswerSound;
        [SerializeField]
        private AudioClip wrongAnswerSound;

        [SerializeField]
        private AudioClip levelCompletedSound;
        [SerializeField]
        private AudioClip jumpscareSound;
        [SerializeField]
        private AudioClip backgroundMusic;

        [SerializeField]
        private Step startObject;
        [SerializeField]
        private Step endObject;

        [SerializeField]
        List<Step> m_StepList = new List<Step>();

        private bool gameEnd = false; // Assuming this exists to track game end condition
        private bool gameEndExecuted = false; // Flag to track if GameEnd has been executed
        private bool playerSurvived = false;
        private bool pronunciationSet;

        // Example method to play a sound
        public void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
            }
        }

        public void PlayThreat(AudioClip clip)
        {
            if (threatAudioSource != null && clip != null)
            {
                threatAudioSource.clip = clip;
                threatAudioSource.Play();
            }
        }

        // Example method to play a sound
        public void PlayBGM(AudioClip clip)
        {
            if (bgmAudioSource != null && clip != null)
            {
                bgmAudioSource.clip = clip;
                bgmAudioSource.Play();
            }
        }

        private List<Question> questions;


        private List<Question> frenchQuestions = new List<Question>
        {
            new Question("Comment dit-on \"Hello\" en français?", new List<string>{"Bonjour", "Au revoir", "Merci"}, 0),
            new Question("Quel est le mot français pour \"Apple\"?", new List<string>{"Pomme", "Banane", "Orange"}, 0),
            new Question("Comment dit-on \"Thank you\" en français?", new List<string>{"Merci", "S'il vous plaît", "De rien"}, 0),
            new Question("Quel est le mot français pour \"Book\"?", new List<string>{"Livre", "Page", "Couverture"}, 0),
            new Question("Comment demande-t-on \"How are you?\" en français?", new List<string>{"Comment allez-vous?", "Où allez-vous?", "Quand allez-vous?"}, 0),
            new Question("Quel est le mot français pour \"Sun\"?", new List<string>{"Soleil", "Lune", "Étoile"}, 0),
            new Question("Comment dit-on \"Goodbye\" en français?", new List<string>{"Au revoir", "Bonjour", "Bonne nuit"}, 0)
        };

        private List<Question> deerQuestions = new List<Question>
        {
            new Question("A group of deer is known as?", new List<string>{"Pack", "Herd", "Flock"}, 1),
            new Question("A baby deer is called?", new List<string>{"Cub", "Fawn", "Pup"}, 1),
            new Question("Deer's natural predator?", new List<string>{"Lion", "Wolf", "Eagle"}, 1),
            new Question("Deer's antlers are made of?", new List<string>{"Bone", "Cartilage", "Keratin"}, 0),
            new Question("Deer's main defensive tactic?", new List<string>{"Camouflage", "Speed", "Aggression"}, 1),
            new Question("Deer's seasonal behavior change?", new List<string>{"Migration", "Hibernation", "Molting"}, 0),
            new Question("Deer communicate through?", new List<string>{"Dance", "Scent", "Song"}, 1),
        };

        private List<Question> canadaQuestions = new List<Question>
        {
            new Question("What is Canada's national animal?", new List<string>{"Beaver", "Moose", "Penguin"}, 0),
            new Question("What do Canadians say after every sentence?", new List<string>{"Eh", "Uh", "Oh"}, 0),
            new Question("Which sport is considered Canada's national pastime?", new List<string>{"Hockey", "Curling", "Snowboarding"}, 0),
            new Question("What is the iconic Canadian coffee shop?", new List<string>{"Tim Hortons", "Starbucks", "Dunkin' Donuts"}, 0),
            new Question("What is a popular Canadian dish featuring fries, cheese curds, and gravy?", new List<string>{"Poutine", "Nachos", "Pizza"}, 0),
            new Question("What is Canada's most famous mountainous national park?", new List<string>{"Banff", "Yellowstone", "Yosemite"}, 0),
            new Question("What do Canadians use to apologize?", new List<string>{"Sorry", "Excuse Me", "Pardon"}, 0),
            new Question("What iconic leaf is on the Canadian flag?", new List<string>{"Maple Leaf", "Oak Leaf", "Pine Leaf"}, 0)
        };


        private List<Question> frenchToEnglishQuestions = new List<Question>
        {
            new Question("What is \"baguette\" in English?", new List<string>{"Breadstick", "Wand", "French stick"}, 0),
            new Question("How do you say \"fromage\" in English?", new List<string>{"Cheese", "Wine", "Cake"}, 0),
            new Question("What is \"escargot\" in English?", new List<string>{"Snail", "Fast food", "Seafood"}, 0),
            new Question("How do you translate \"lunettes\" into English?", new List<string>{"Glasses", "Moon boots", "Windows"}, 0),
            new Question("What does \"omelette du fromage\" mean in English?", new List<string>{"Cheese omelet", "French toast", "Milkshake"}, 0),
            new Question("Translate \"pamplemousse\" to English.", new List<string>{"Grapefruit", "Pineapple", "Watermelon"}, 0),
            new Question("What is \"sacré bleu\" in English?", new List<string>{"Holy smokes", "Blue bag", "Sacred blue"}, 0),
            new Question("How do you say \"pantoufles\" in English?", new List<string>{"Slippers", "Pants", "Flowers"}, 0),
            new Question("What does \"chat\" translate to in English?", new List<string>{"Cat", "Chat", "Hat"}, 0),
            new Question("Translate \"quiche\" into English.", new List<string>{"Quiche", "Cake", "Pie"}, 0)
        };

        private List<Question> mathQuestions = new List<Question>
        {
            new Question("What is 6 * 7?", new List<string>{"42", "52", "48"}, 0),
            new Question("What is the square root of 144?", new List<string>{"12", "14", "16"}, 0),
            new Question("If x + 2 = 14, what is x?", new List<string>{"12", "10", "16"}, 0),
            new Question("What is the sum of the angles in a triangle?", new List<string>{"180 degrees", "360 degrees", "90 degrees"}, 0),
            new Question("What is 15% of 200?", new List<string>{"30", "25", "40"}, 0),
            new Question("What is the area of a circle with a radius of 4 units?", new List<string>{"16π", "8π", "32π"}, 0),
            new Question("Solve for x: 3x - 9 = 0", new List<string>{"3", "9", "6"}, 0)
        };


        private List<Question> programmingQuestions = new List<Question>
        {
            new Question("In programming, what does 'IDE' stand for?", new List<string>{"Integrated Development Environment", "Internal Data Exchange", "Intelligent Design Engine"}, 0),
            new Question("What is the term for making an instance of a class in programming?", new List<string>{"Instantiation", "Declaration", "Initialization"}, 0),
            new Question("Which of the following is a valid variable name in Python?", new List<string>{"_myVar", "2myVar", "-myVar"}, 0),
            new Question("What encoding is used to represent text in which each symbol is represented by 16 bits?", new List<string>{"UTF-16", "ASCII", "UTF-8"}, 0),
            new Question("What HTML tag is used to define an image?", new List<string>{"<img>", "<image>", "<pic>"}, 0),
            new Question("In CSS, what property is used to change the text color of an element?", new List<string>{"color", "font-color", "text-color"}, 0),
            new Question("What does 'SQL' stand for?", new List<string>{"Structured Query Language", "Standard Query Language", "Simple Query Language"}, 0)
        };

        private List<Question> scienceQuestions = new List<Question>
        {
            new Question("What is the powerhouse of the cell?", new List<string>{"Mitochondria", "Nucleus", "Ribosome"}, 0),
            new Question("What element is known as the 'building block of life'?", new List<string>{"Carbon", "Oxygen", "Hydrogen"}, 0),
            new Question("Which planet is known as the 'Red Planet'?", new List<string>{"Mars", "Jupiter", "Venus"}, 0),
            new Question("What is the most abundant gas in the Earth's atmosphere?", new List<string>{"Nitrogen", "Oxygen", "Carbon Dioxide"}, 0),
            new Question("What do you call the process by which plants make their food?", new List<string>{"Photosynthesis", "Respiration", "Fermentation"}, 0),
            new Question("Who is known as the father of modern physics?", new List<string>{"Albert Einstein", "Isaac Newton", "Nikola Tesla"}, 0),
            new Question("What is the term for animals that can live both in water and on land?", new List<string>{"Amphibians", "Reptiles", "Mammals"}, 0)
        };
        List<string> correctAnswersThreats = new List<string>
        {
            "\"Five more correct answers, and we might consider untying your family. No pressure.\"",
            "\"Impressive! Your pet's safety is guaranteed... for another day.\"",
            "\"Excellent! Your friends will be thrilled to hear they're not next on our list.\"",
            "\"Bravo! We've decided to postpone your plant's mysterious disappearance.\"",
            "\"Right again! Your favorite teddy bear will not be harmed today.\"",
            "\"Such talent! Perhaps your internet connection will remain stable for your next lesson.\"",
            "\"Marvelous! We might just send a postcard from your family's unplanned 'extended vacation.'\"",
        };


        List<string> incorrectAnswersThreats = new List<string>
        {
            "\"Incorrect. We've heard your family was planning a vacation. It'd be a shame if they went without you.\"",
            "\"Keep it up, and you may just earn yourself a visitation right to your own house.\"",

            "\"Another wrong answer? We hope you weren't too attached to your pet fish.\"",
            "\"Oh no, wrong again. Did you hear that? Sounds like the lock on your backdoor turning.\"",
            "\"Another mistake? Seems like your alarm clock may find itself set for 3 AM... again.\"",
            "\"Wrong. Just for that, we're thinking of a new home for your cherished houseplant.\"",
            "\"Miscalculation detected. Maybe it's time your thermostat developed a mind of its own.\"",
        };



        int m_CurrentStepIndex;

        int numberOfCorrectAnswers;
        int numberOfWrongAnswers;



        private bool startGameClicked = false;

        void TextToSpeech(string text)
        {
            //voice.Rate = 4;
            //voice.Speak(text, SpeechVoiceSpeakFlags.SVSFlagsAsync | SpeechVoiceSpeakFlags.SVSFPurgeBeforeSpeak);
            Debug.Log(text);
        }

        void Start()
        {
            jumpscareObject.SetActive(false);
            gameEnd = false;
            gameEndExecuted = false;
            m_CurrentStepIndex = 0;
            numberOfWrongAnswers = 0;
            numberOfCorrectAnswers = 0;
            playerSurvived = false;
            startGameClicked = false;
            pronunciationSet = false;
            UpdateBirdPositions(new Vector3(20f, 1.05f, 0f)); // Assuming this method correctly positions the bird


            endObject.stepObject.SetActive(false);
            m_StepList[0].stepObject.SetActive(false);



            TMP_Dropdown currentDropdown = startObject.dropdownObject;
            if (currentDropdown != null)
            {
                currentDropdown.ClearOptions();
                currentDropdown.AddOptions(new List<string> { "Oui Oui Baguette Basics (French)", "Your younger sister's math homework", "Why won't you compile (CS)", "Deer(Hacks) Facts", "Mitochondria Things (Science)", "Aboot & Maple Syrup Knowledge" });
            }

            

            startObject.stepObject.SetActive(true);

        }

        void PopulateQuestions(List<Question> questions)
        {
            for (int i = 0; i < m_StepList.Count && i < questions.Count; i++)
            {
                var step = m_StepList[i];
                var question = questions[i];

                if (step.textObject != null)
                    step.textObject.text = question.questionText;

                if (step.dropdownObject != null)
                {
                    List<string> randomizedAnswers = question.answers.OrderBy(a => System.Guid.NewGuid()).ToList();

                    step.dropdownObject.ClearOptions();
                    step.dropdownObject.AddOptions(randomizedAnswers);
                }
            }
        }

        public float moveSpeed; // Units per second

        void UpdateBirdPositions(Vector3 vector)
        {
            controlledObjectTransform.position = vector;
        }

        void GameEnd()
        {
            Debug.Log("GameEnd called.");
            endObject.stepObject.SetActive(true); // Make sure endObject is assigned and has a stepObject to activate


            if (playerSurvived)
            {
                Debug.Log("Player survived.");

                if (endObject.textObject != null)
                    endObject.textObject.text = "You survived!";

                PlayBGM(levelCompletedSound);
                UpdateBirdPositions(new Vector3(20f, 1.05f, 0f)); // Assuming this method correctly positions the bird
            }
            else
            {
                Debug.Log("Player dead.");
                m_StepList[m_CurrentStepIndex].stepObject.SetActive(false);

                if (endObject.textObject != null)
                    endObject.textObject.text = "You died!";

                PlayBGM(jumpscareSound);
                // program specific bird to appear
                jumpscareObject.SetActive(true);
                // UpdateBirdPositions(new Vector3(1f, -3f, 0.7f)); // Assuming this method correctly positions the bird
            }
        }

        void Update()
        {
            // Move the object continuously along the X-axis if game hasn't ended
            if (controlledObjectTransform != null && !gameEnd && startGameClicked)
            {
                controlledObjectTransform.position += new Vector3(moveSpeed * Time.deltaTime, 0, 0);
            }

            // Check if game should end
            if ((controlledObjectTransform.position.x < 0) && !gameEnd)
            {
                gameEnd = true;
                playerSurvived = false;
            }

            // Call GameEnd() once when gameEnd is true and it hasn't been executed yet
            if (gameEnd && !gameEndExecuted)
            {
                GameEnd();
                gameEndExecuted = true; // Prevents GameEnd from being called again
            }
        }

        public void Next()
        {
            if (gameEnd)
            {
                Start();
            }
            else if (!startGameClicked)
            {
                PlayBGM(backgroundMusic);

                TMP_Dropdown currentDropdown = startObject.dropdownObject;
                int selectedAnswerIndex = currentDropdown.value;

                Debug.Log("selected topic: " + currentDropdown.options[selectedAnswerIndex].text);

                switch (selectedAnswerIndex)
                {
                    case 0:
                        questions = frenchToEnglishQuestions;
                        break;
                    case 1:
                        questions = (mathQuestions);
                        break;
                    case 2:
                        questions = (programmingQuestions);
                        break;
                    case 3:
                        questions = deerQuestions;
                        break;
                    case 4:
                        questions = scienceQuestions;
                        break;
                    case 5:
                        questions = canadaQuestions;
                        break;
                    default:
                        questions = (frenchQuestions);
                        break;

                }
                PopulateQuestions(questions);

                startGameClicked = true;
                startObject.stepObject.SetActive(false);
                m_StepList[0].stepObject.SetActive(true);
            }
            else if (m_StepList[m_CurrentStepIndex].dropdownObject != null)
            {
                TMP_Dropdown currentDropdown = m_StepList[m_CurrentStepIndex].dropdownObject;
                int selectedAnswerIndex = currentDropdown.value;
                string selectedAnswer = currentDropdown.options[selectedAnswerIndex].text.ToLower();

                Question currentQuestion = questions[m_CurrentStepIndex];

                // Check if the selected answer is correct
                string correctAnswer = currentQuestion.answers[currentQuestion.correctAnswerIndex].ToLower();

                bool isCorrect = selectedAnswer == correctAnswer;

                Debug.Log("Correct answer: " + correctAnswer + ", Selected Answer: " + selectedAnswer + ", answer in transcript: " + isCorrect);


                // if (pronunciationSet)
                // {
                //     string lowercaseTranscriptText = transcriptText.text.ToLower();

                //     Debug.Log("Correct answer: " + correctAnswer + ", Recorded Text: " + transcriptText.text + ", answer in transcript: " + lowercaseTranscriptText.Contains(correctAnswer));

                //     isCorrect = lowercaseTranscriptText.Contains(correctAnswer);
                // }

                // Log the result or show a popup
                if (isCorrect)
                {
                    PlaySound(correctAnswerSound);

                    answerResultText.text = correctAnswersThreats[numberOfCorrectAnswers % correctAnswersThreats.Count] + " - Duolingo :)";
                    PlayThreat(correctAnswerAudio[numberOfCorrectAnswers % correctAnswerAudio.Count]);
                    numberOfCorrectAnswers += 1;


                    // Show a popup for correct answer
                    //ShowPopup("Correct!", true);


                    if (controlledObjectTransform != null)
                    {
                        // Move the object -1 unit in the x-axis
                        UpdateBirdPositions(controlledObjectTransform.position + new Vector3(1, 0, 0));
                    }
                    else
                    {
                        Debug.LogError("Controlled Object Transform is not assigned.");
                    }

                    // Deactivate the current step
                    m_StepList[m_CurrentStepIndex].stepObject.SetActive(false);

                    // Move to the next step
                    m_CurrentStepIndex = (m_CurrentStepIndex + 1);

                    if (m_CurrentStepIndex == m_StepList.Count)
                    {
                        playerSurvived = true;
                        gameEnd = true;
                    }

                    // Activate the next step
                    m_StepList[m_CurrentStepIndex].stepObject.SetActive(true);
                 
                    // Update the step button text
                    m_StepButtonTextField.text = m_StepList[m_CurrentStepIndex].buttonText;

                    //if (pronunciationSet)
                    //{
                    //    m_StepList[m_CurrentStepIndex].dropdownObject.SetActive(false);
                    //}
                }
                else
                {
                    PlaySound(wrongAnswerSound);

                    answerResultText.text = incorrectAnswersThreats[numberOfWrongAnswers % incorrectAnswerAudio.Count] + " - Duolingo :)";
                    PlayThreat(incorrectAnswerAudio[numberOfWrongAnswers % incorrectAnswerAudio.Count]);

                    numberOfWrongAnswers += 1;

                    // Show a popup for wrong answer
                    //ShowPopup("Wrong!", false);

                    if (controlledObjectTransform != null)
                    {
                        // Move the object -1 unit in the x-axis
                        UpdateBirdPositions(controlledObjectTransform.position + new Vector3(-2, 0, 0));
                    }
                    else
                    {
                        Debug.LogError("Controlled Object Transform is not assigned.");
                    }
                }
            }
            else
            {
                Debug.Log("Step Dropdown GameObject is null.");
            }
        }
    }
}
