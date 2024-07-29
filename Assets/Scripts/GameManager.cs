using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace MatchThreeEngine
{
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private TypeAssetTile[] tileTypes;
        [SerializeField] private TypeRow[] rows;
        [SerializeField] private AudioClip matchSound;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private float tweenDuration;
        [SerializeField] private Transform swappingOverlay;
        [SerializeField] private bool ensureNoStartingMatches;
        [SerializeField] private Slider timerSlider;
        [SerializeField] private float gameDuration = 120f;
        [SerializeField] private TMP_Text winTimeText;

        private readonly List<TypeTile> _selection = new List<TypeTile>();
        private bool _isSwapping;
        public GameObject EventGame;
        private bool _isMatching;
        private bool _isShuffling;
        private bool _isGameRunning;
        public TMP_Text scoreGame;
        public TMP_Text scoreLose;
        public int score;
        public GameObject winPanel;
        public GameObject lostPanel;
        private float _timeRemaining;
        private Coroutine _timerCoroutine;
        public int currentLevel = 1;
        public TMP_Text currentLevelText;
        public Button[] levelButtons;
        public TMP_Text winCurrentLevelText;
        public TMP_Text loseCurrentLevelText;
        public int levels = 1;
        public GameObject levelMenu;
        public TMP_Text winTime;
        [SerializeField] private TMP_Text timerText;

        public event Action<TypeAssetTile, int> OnMatch;

        private TileData[,] GameControll
        {
            get
            {
                var width = rows.Max(row => row.tiles.Length);
                var height = rows.Length;

                var data = new TileData[width, height];

                for (var y = 0; y < height; y++)
                    for (var x = 0; x < width; x++)
                        data[x, y] = GetTile(x, y).Data;

                return data;
            }
        }


        /// Методы для управления уровнями
		
        public void OnLevelButtonPressed(int selectedLevel)
        {
            // Устанавливаем текущий уровень на выбранный уровень
            currentLevel = selectedLevel;
            // Скрываем меню выбора уровня
            levelMenu.SetActive(false);
            // Запускаем игру
            StartGame();
        }


        public void ExitGame()
        {
            Application.Quit();
        }

        void SaveCompletedLevels()
        {
            // Сохраняем количество завершенных уровней в PlayerPrefs
            PlayerPrefs.SetInt("CompletedLevelsCount", levels);
            // Сохраняем изменения
            PlayerPrefs.Save();
        }


        void Start()
        {
            InitializeGame();
        }

        private void InitializeGame()
        {
            // Загружаем завершенные уровни
            LoadCompletedLevels();
    
            // Обновляем состояние интерактивности кнопок уровней
            UpdateLevelButtonInteractivity();
        }


        public void AdvanceToNextLevel()
        {
            // Проверяем, можно ли перейти на следующий уровень
            if (currentLevel < levels)
            {
                // Увеличиваем текущий уровень на 1
                currentLevel++;
            }
            // Обновляем текст текущего уровня
            currentLevelText.text = $"Level {currentLevel}";
            // Сбрасываем доску для следующего уровня
            ResetBoardForNewLevel();
            // Запускаем игру
            StartGame();
            currentLevelText.text = $"Level {currentLevel}";

        }


        void ResetBoardForNewLevel()
        {
            // Сбрасываем счет на ноль
            score = 0;
            // Обновляем текст счета
            scoreGame.text = $"Points: {score}/40";
        }

        void LoadCompletedLevels()
        {
            levels = GetCompletedLevels();
        }

        private int GetCompletedLevels()
        {
            // Получаем количество завершенных уровней из PlayerPrefs, по умолчанию 1
            return PlayerPrefs.GetInt("LevelsCompleted", 1);
        }


        void UpdateLevelButtonInteractivity()
        {
            // Обновляем состояние кнопок уровней
            for (int buttonIndex = 0; buttonIndex < levelButtons.Length; buttonIndex++)
            {
                // Устанавливаем доступность кнопки в зависимости от завершенных уровней
                levelButtons[buttonIndex].interactable = buttonIndex < levels;
            }
        }

        public void StartGame()
        {
            SetupGameTiles();
            ResetScoreAndDisplay();
            CheckForVictoryCondition();
            ContinueGameInitialization();
            currentLevelText.text = $"Level {currentLevel}";

        }

        private void SetupGameTiles()
        {
            for (var y = 0; y < rows.Length; y++)
            {
                for (var x = 0; x < rows[y].tiles.Length; x++)
                {
                    var tile = GetTile(x, y);

                    tile.x = x;
                    tile.y = y;

                    tile.Type = tileTypes[Random.Range(0, tileTypes.Length)];

                    tile.button.onClick.AddListener(() => Select(tile));
                }
            }
        }

        private void ResetScoreAndDisplay()
        {
            score = 0;
            scoreGame.text = $"Points: {score}/40";
        }

        private void CheckForVictoryCondition()
        {
            if (score >= 40)
            {
                GameWon();
                return;
            }
        }

        private void ContinueGameInitialization()
        {
            if (ensureNoStartingMatches)
            {
                StartCoroutine(NoStartingMatches());
            }

            StopTimer();
            _timeRemaining = gameDuration;
            timerSlider.maxValue = gameDuration;
            _isGameRunning = true;
            _timerCoroutine = StartCoroutine(TimerCoroutine());
        }



        private IEnumerator TimerCoroutine()
        {
            // Цикл продолжается, пока остается время и игра активна
            while (_timeRemaining > 0 && _isGameRunning)
            {
                // Уменьшаем оставшееся время на прошедшее время с последнего кадра
                _timeRemaining -= Time.deltaTime;
                UpdateTimerDisplay();

                // Если время вышло, завершаем игру
                if (_timeRemaining <= 0)
                {
                    HandleGameEnd();
                }

                // Ждем до следующего кадра
                yield return null;
            }
        }

        private void UpdateTimerDisplay()
        {
            // Устанавливаем значение слайдера и обновляем текст таймера
            timerSlider.value = _timeRemaining;
            int seconds = (int)_timeRemaining;
            timerText.text = $"Time: {seconds}s";
        }

        private void HandleGameEnd()
        {
            // Обработка окончания игры при истечении времени
            GameLost();
        }
        
        public void StopTimer()
        {
            // Устанавливаем флаг, что игра больше не выполняется
            _isGameRunning = false;
    
            // Проверяем, что корутина таймера существует и останавливаем её
            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
                _timerCoroutine = null; // Обнуляем переменную корутины, чтобы предотвратить возможные ошибки
            }
        }


        public void StartTimer()
        {
            // Проверяем, что игра не уже выполняется
            if (!_isGameRunning)
            {
                // Устанавливаем флаг, что игра выполняется
                _isGameRunning = true;
        
                // Запускаем корутину таймера
                _timerCoroutine = StartCoroutine(TimerCoroutine());
            }
            else
            {
                Debug.LogWarning("Работает");
            }
        }

        
        public void UpdateGameScore()
        {
            if (score >= 40)
                GameWon();
        }
        
        public void AfterGameWin()
        {
            // Увеличиваем уровень, если это не последний уровень
            if (levels < 28)
            {
                levels++;
            }
            // Сохраняем текущее количество завершенных уровней
            SaveCompletedLevels();
            // Обновляем интерактивность кнопок уровней
            UpdateLevelButtonInteractivity();
        }

        
        void GameWon()
        {
            Debug.Log("Game won!");

            // Обновление текста в winPanel
            if (winPanel != null)
            {
                var winTextComponent = winPanel.GetComponentInChildren<TMP_Text>();
                if (winTextComponent != null)
                {
                    winTextComponent.text = $"Points: {score}/40";
                }
                else
                {
                    Debug.LogError("TMP_Text не установлен в инспекторе!!");
                }

                // Обновление времени
                if (winTimeText != null)
                {
                    int secondsRemaining = Mathf.FloorToInt(_timeRemaining);
                    winTimeText.text = $"Time: {secondsRemaining}s";
                }
                else
                {
                    Debug.LogError("winTimeText не установлен в инспекторе!!");
                }
        
                // Обновление текста текущего уровня
                if (winCurrentLevelText != null)
                {
                    winCurrentLevelText.text = $"Level: {currentLevel}";
                }
                else
                {
                    Debug.LogError("winCurrentLevelText не установлен в инспекторе!!");
                }

                winPanel.SetActive(true);
            }
            else
            {
                Debug.LogError("winPanel не установлен в инспекторе!!");
            }

            StopTimer();
        }




        
        void GameLost()
        {
            if (lostPanel != null)
            {
                // Установка текста итоговых очков
                scoreLose.text = $"Points: {score}/40";

                // Обновление текста текущего уровня
                if (loseCurrentLevelText != null)
                {
                    loseCurrentLevelText.text = $"Level: {currentLevel}";
                }
                else
                {
                    Debug.LogError("loseCurrentLevelText не установлен в инспекторе");
                }

                lostPanel.SetActive(true);
            }
            else
            {
                Debug.LogError("lostPanel не установлен в инспекторе!");
            }

            StopTimer();
        }



        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                var bestMove = TileDataControllUtility.SearchBestMove(GameControll);

                if (bestMove != null)
                {
                    Select(GetTile(bestMove.X1, bestMove.Y1));
                    Select(GetTile(bestMove.X2, bestMove.Y2));
                }
            }
            UpdateGameScore();
        }

        private IEnumerator NoStartingMatches()
        {
            var wait = new WaitForEndOfFrame();

            // Пока есть начальные совпадения, перемешиваем элементы игры
            while (TileDataControllUtility.SearchBestMatch(GameControll) != null)
            {
                Shuffle();  // Перемешивание элементов игры
                yield return wait;  // Ждем конца кадра перед следующей итерацией
            }
            
        }


        private TypeTile GetTile(int x, int y) => rows[y].tiles[x];

        private TypeTile[] GetTiles(IList<TileData> tileData)
        {
            var length = tileData.Count;

            var tiles = new TypeTile[length];

            for (var i = 0; i < length; i++) tiles[i] = GetTile(tileData[i].X, tileData[i].Y);

            return tiles;
        }

        private async void Select(TypeTile tile)
        {
            // Проверяем, выполняются ли другие действия; если да, игнорируем выбор
            if (_isSwapping || _isMatching || _isShuffling)
            {
                Debug.Log("Action in progress, selection ignored.");
                return;
            }

            // Добавляем выбранный тайл в коллекцию выбора, если его там нет
            if (!_selection.Contains(tile))
            {
                if (_selection.Count > 0)
                {
                    // Проверяем, можно ли обменять выбранный тайл с первым в коллекции
                    if (Math.Abs(tile.x - _selection[0].x) == 1 && Math.Abs(tile.y - _selection[0].y) == 0
                        || Math.Abs(tile.y - _selection[0].y) == 1 && Math.Abs(tile.x - _selection[0].x) == 0)
                    {
                        _selection.Add(tile);
                    }
                }
                else
                {
                    _selection.Add(tile);
                }
            }

            // Если в коллекции меньше двух элементов, выходим
            if (_selection.Count < 2) return;

            // Устанавливаем флаг обмена в true и выполняем обмен асинхронно
            _isSwapping = true;
            bool success = await SwapAndMatchAsync(_selection[0], _selection[1]);
    
            // Если обмен не удался, просто выполняем обмен без проверки сопоставления
            if (!success)
            {
                await SwapAsync(_selection[0], _selection[1]);
            }
    
            // Сбрасываем коллекцию выбора и обеспечиваем играбельность доски
            _isSwapping = false;
            _selection.Clear();
            EnsurePlayableBoard();
        }


        private async Task<bool> SwapAndMatchAsync(TypeTile tile1, TypeTile tile2)
        {
            await SwapAsync(tile1, tile2);
            if (await TryMatchAsync())
            {
                return true;
            }
            return false;
        }

        private async Task SwapAsync(TypeTile tile1, TypeTile tile2)
        {
            var icon1 = tile1.icon;
            var icon2 = tile2.icon;

            var icon1Transform = icon1.transform;
            var icon2Transform = icon2.transform;

            icon1Transform.SetParent(swappingOverlay);
            icon2Transform.SetParent(swappingOverlay);

            icon1Transform.SetAsLastSibling();
            icon2Transform.SetAsLastSibling();

            icon1Transform.SetParent(tile2.transform);
            icon2Transform.SetParent(tile1.transform);

            tile1.icon = icon2;
            tile2.icon = icon1;

            var tile1Item = tile1.Type;
            tile1.Type = tile2.Type;
            tile2.Type = tile1Item;
        }

        private void EnsurePlayableBoard()
        {
            var matrix = GameControll;

            while (TileDataControllUtility.SearchBestMove(matrix) == null || TileDataControllUtility.SearchBestMatch(matrix) != null)
            {
                Shuffle();
                matrix = GameControll;
            }
        }

        private async Task<bool> TryMatchAsync()
        {
            var didMatch = false;

            _isMatching = true;

            var match = TileDataControllUtility.SearchBestMatch(GameControll);

            while (match != null)
            {
                didMatch = true;

                var tiles = GetTiles(match.Tiles);

                var deflateSequence = DOTween.Sequence();

                foreach (var tile in tiles)
                    deflateSequence.Join(tile.icon.transform.DOScale(Vector3.zero, tweenDuration).SetEase(Ease.InBack));

                audioSource.PlayOneShot(matchSound);

                await deflateSequence.Play().AsyncWaitForCompletion();

                foreach (var tile in tiles)
                    tile.Type = tileTypes[Random.Range(0, tileTypes.Length)];

                var inflateSequence = DOTween.Sequence();

                foreach (var tile in tiles)
                    inflateSequence.Join(tile.icon.transform.DOScale(Vector3.one, tweenDuration).SetEase(Ease.OutBack));

                await inflateSequence.Play().AsyncWaitForCompletion();

                OnMatch?.Invoke(tiles[0].Type, tiles.Length);

                // Increase score for the found match
                int matchPoints = 1;
                if (tiles.Length >= 5)
                {
                    matchPoints += 5; // Additional 5 points for 4 or more tiles
                }

                score += matchPoints;
                scoreGame.text = "Score: " + score + "/40"; // Update score text

                match = TileDataControllUtility.SearchBestMatch(GameControll);
            }

            _isMatching = false;

            return didMatch;
        }

        private void Shuffle()
        {
            _isShuffling = true;

            foreach (var row in rows)
                foreach (var tile in row.tiles)
                    tile.Type = tileTypes[Random.Range(0, tileTypes.Length)];

            _isShuffling = false;
        }

        public void RefreshBoard()
        {
            // Сохраняем текущее состояние таймера и очков
            float remainingTime = _timeRemaining;
            int currentScore = score;

            // Логика обновления доски игры
            ResetBoardForNewLevel(); // Сбросить доску для следующего уровня

            // Восстанавливаем состояние очков
            score = currentScore;
            scoreGame.text = $"Points: {score}/40";

            // Запускаем игру заново после обновления
            StartGame();

            // Восстанавливаем состояние таймера
            _timeRemaining = remainingTime;
            timerSlider.value = _timeRemaining;
            StartCoroutine(TimerCoroutine()); // Запускаем таймер заново
        }

        
        public void ButtonActivated()
        {
            EventGame.SetActive(false);
            StartCoroutine("ResetEvent", 0.1f);
        }

        public void ResetEvent()
        {
            EventGame.SetActive(true);
        }
    }
}
