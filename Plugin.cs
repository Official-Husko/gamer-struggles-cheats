using BepInEx;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace gs_cheat_menu
{
    [BepInPlugin("husko.gamerstruggles.cheats", "Gamer Struggles Cheats", MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private enum Tab
        {
            MainCheats,
            SpecialCheats
        }

        private Tab _currentTab = Tab.MainCheats;
        private bool _showMenu;
        private Rect _menuRect = new(20, 20, 330, 200); // Initial position and size of the menu

        // Define separate arrays to store activation status for each tab
        private readonly bool[] _mainCheatsActivated = new bool[3];
        private readonly bool[] _specialCheatsActivated = new bool[0]; // Adjust the size as per your requirement

        // Default values
        private int _healthValue = 5;
        private int _staminaValue = 9;

        private const string VersionLabel = MyPluginInfo.PLUGIN_VERSION;

        // List to store button labels and corresponding actions for the current cheats tab
        private readonly List<(string label, Action action)> _mainCheatsButtonActions = new()
        {
            ("Toggle Camera Effects", TogglePostProcessingEffects),
            ("Toggle Stamina Drain", ToggleStaminaDrain),
            ("Toggle Enemy Damage", ToggleEnemyDamage),
            // Add more buttons and actions here
        };

        // Modify the ghostModeButtonActions list to include a button for Special Cheats
        private readonly List<(string label, Action action)> _specialCheatsButtonActions = new()
        {
            // Add more buttons for Special Cheats here
        };

        /// <summary>
        /// Initializes the plugin on Awake event
        /// </summary>
        private void Awake()
        {
            // Log the plugin's version number and successful startup
            Logger.LogInfo($"Plugin Gamer Struggles Cheat v{VersionLabel} loaded!");
        }

        /// <summary>
        /// Handles toggling the menu on and off with the Insert or F1 key.
        /// </summary>
        private void Update()
        {
            // Toggle menu visibility with Insert or F1 key
            if (Keyboard.current.insertKey.wasPressedThisFrame || Keyboard.current.f1Key.wasPressedThisFrame)
            {
                _showMenu = !_showMenu;
            }
        }

        /// <summary>
        /// Handles drawing the menu and all of its elements on the screen.
        /// </summary>
        private void OnGUI()
        {
            // Only draw the menu if it's supposed to be shown
            if (_showMenu)
            {
                // Apply dark mode GUI style
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f);

                // Draw the IMGUI window
                _menuRect = GUI.Window(0, _menuRect, MenuWindow, "----< Cheats Menu >----");

                // Calculate position for version label at bottom left corner
                float versionLabelX = _menuRect.xMin + 10; // 10 pixels from left edge
                float versionLabelY = _menuRect.yMax - 20; // 20 pixels from bottom edge

                // Draw version label at bottom left corner
                GUI.contentColor = new Color(0.5f, 0.5f, 0.5f); // Dark grey silver color
                GUI.Label(new Rect(versionLabelX, versionLabelY, 100, 20), "v" + VersionLabel);

                // Calculate the width of the author label
                float authorLabelWidth =
                    GUI.skin.label.CalcSize(new GUIContent("by Official-Husko")).x +
                    10; // Add some extra width for padding

                // Calculate position for author label at bottom right corner
                float authorLabelX = _menuRect.xMax - authorLabelWidth; // 10 pixels from right edge
                float authorLabelY = versionLabelY + 2; // Align with version label

                // Draw the author label as a clickable label
                if (GUI.Button(new Rect(authorLabelX, authorLabelY, authorLabelWidth, 20),
                        "<color=cyan>by</color> <color=yellow>Official-Husko</color>", GUIStyle.none))
                {
                    // Open a link in the user's browser when the label is clicked
                    Application.OpenURL("https://github.com/Official-Husko/gamer-struggles-cheats");
                }
            }
        }

        /// <summary>
        /// Handles the GUI for the main menu
        /// </summary>
        /// <param name="windowID">The ID of the window</param>
        private void MenuWindow(int windowID)
        {
            // Make the whole window draggable
            GUI.DragWindow(new Rect(0, 0, _menuRect.width, 20));

            // Begin a vertical group for menu elements
            GUILayout.BeginVertical();

            // Draw tabs
            GUILayout.BeginHorizontal();
            // Draw the Main Cheats tab button
            DrawTabButton(Tab.MainCheats, "Main Cheats");
            // Draw the Special Cheats tab button
            DrawTabButton(Tab.SpecialCheats, "Special Cheats");
            GUILayout.EndHorizontal();

            // Draw content based on the selected tab
            switch (_currentTab)
            {
                // Draw the Main Cheats tab
                case Tab.MainCheats:
                    DrawMainCheatsTab();
                    break;
                // Draw the Special Cheats tab
                case Tab.SpecialCheats:
                    DrawSpecialCheatsTab();
                    break;
            }

            // End the vertical group
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draws a tab button
        /// </summary>
        /// <param name="tab">The tab to draw</param>
        /// <param name="label">The label to display on the button</param>
        private void DrawTabButton(Tab tab, string label)
        {
            // Change background color based on the selected tab
            GUI.backgroundColor = _currentTab == tab ? Color.white : Color.grey;

            // If the button is clicked, set the current tab to the clicked tab
            if (GUILayout.Button(label))
            {
                _currentTab = tab;
            }
        }

        /// <summary>
        /// Gets the activation status array for the currently selected tab
        /// </summary>
        /// <returns>The activation status array for the current tab. If the tab is not recognized, null is returned.</returns>
        private bool[] GetCurrentTabActivationArray()
        {
            switch (_currentTab)
            {
                case Tab.MainCheats:
                    // Return the activation status array for the main cheats tab
                    return _mainCheatsActivated;
                case Tab.SpecialCheats:
                    // Return the activation status array for the special cheats tab
                    return _specialCheatsActivated;
                default:
                    // If the tab is not recognized, return null
                    return null;
            }
        }

        /// <summary>
        /// Toggles the activation state of the button at the given index on the currently selected tab.
        /// If the index is not within the range of the activation status array for the current tab, nothing is done.
        /// </summary>
        /// <param name="buttonIndex">The index of the button to toggle activation status for</param>
        private void ToggleButtonActivation(int buttonIndex)
        {
            // Get the activation status array for the current tab. If the tab is not recognized, return.
            bool[] currentTabActivationArray = GetCurrentTabActivationArray();
            if (currentTabActivationArray == null)
            {
                return;
            }

            // If the index is within the range of the activation status array, toggle the activation status
            if (buttonIndex >= 0 && buttonIndex < currentTabActivationArray.Length)
            {
                currentTabActivationArray[buttonIndex] = !currentTabActivationArray[buttonIndex];
            }
        }

        /// <summary>
        /// Method to draw content for the Main Cheats tab
        /// </summary>
        private void DrawMainCheatsTab()
        {
            GUILayout.BeginVertical();

            // Draw buttons from the list
            for (int i = 0; i < _mainCheatsButtonActions.Count; i++)
            {
                GUILayout.BeginHorizontal();
                DrawActivationDot(_mainCheatsActivated[i]); // Draw activation dot based on activation status

                // Draws a button for each cheat with the label, 
                // activation status, and invokes the action associated 
                // with the button when pressed
                if (GUILayout.Button(_mainCheatsButtonActions[i].label))
                {
                    ToggleButtonActivation(i); // Toggle activation status
                    _mainCheatsButtonActions[i].action.Invoke(); // Invoke the action associated with the button
                }

                GUILayout.EndHorizontal();
            }
            
            // Draw infinite stamina button
            DrawInfiniteStaminaButton();
            
            // Draw infinite health button
            DrawInfiniteHealthButton();
            
            // End the vertical layout
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draws the Special Cheats tab in the mod's UI
        /// </summary>
        private void DrawSpecialCheatsTab()
        {
            // Begin vertical layout for the tab
            GUILayout.BeginVertical();

            // Iterate through the list of special cheat buttons
            for (int i = 0; i < _specialCheatsButtonActions.Count; i++)
            {
                // Begin horizontal layout for the button row
                GUILayout.BeginHorizontal();

                // Draw an activation dot based on the activation status
                DrawActivationDot(_specialCheatsActivated[i]);

                // Draw a button for the special cheat
                if (GUILayout.Button(_specialCheatsButtonActions[i].label))
                {
                    // Toggle the activation status of the button
                    ToggleButtonActivation(i);

                    // Invoke the action associated with the button
                    _specialCheatsButtonActions[i].action.Invoke();
                }

                // End the horizontal layout for the button row
                GUILayout.EndHorizontal();
            }

            // Draw unlock gallery buttons
            UnlockGalleryButtons();
            
            // End the vertical layout for the tab
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draws a small dot with a green color if the activation status is true, and red if it's false.
        /// This method uses the current tab activation status array to determine the dot color.
        /// </summary>
        /// <param name="activated">The activation status to determine the dot color.</param>
        private void DrawActivationDot(bool activated)
        {
            GetCurrentTabActivationArray(); // Consider current tab activation status array
            GUILayout.Space(10); // Add some space to center the dot vertically
            Color dotColor = activated ? Color.green : Color.red; // Determine dot color based on activation status
            GUIStyle dotStyle = new GUIStyle(GUI.skin.label); // Create a new GUIStyle for the dot label
            dotStyle.normal.textColor = dotColor; // Set the color of the dot label
            GUILayout.Label("●", dotStyle, GUILayout.Width(20),
                GUILayout.Height(20)); // Draw dot with the specified style
        }

        private void DrawBlueDot()
        {
            GUILayout.Space(10); // Add some space to center the dot vertically
            Color blueDotColor = new Color(0.0f, 0.5f, 1.0f); // blue because nice
            GUIStyle dotStyle = new GUIStyle(GUI.skin.label); // Create a new GUIStyle for the dot label
            dotStyle.normal.textColor = blueDotColor; // Set the color of the dot label
            GUILayout.Label("●", dotStyle, GUILayout.Width(20), GUILayout.Height(20)); // Draw dot with the specified style
        }
        
        /*
         Below here are all the code related things for the cheats itself.
        */

        private static void TogglePostProcessingEffects()
        {
            // Debug log the action being performed
            Debug.Log("Toggle Camera Effects");

            // Find the "PlayerStuff(Clone)" GameObject
            GameObject playerStuff = GameObject.Find("PlayerStuff(Clone)");
            if (playerStuff != null)
            {
                // Find the "Main Camera" GameObject within "PlayerStuff(Clone)"
                Transform cameraHolder = playerStuff.transform.Find("Camera Holder");
                if (cameraHolder != null)
                {
                    Transform mainCamera = cameraHolder.Find("Main Camera");
                    if (mainCamera != null)
                    {
                        // Try to get the PostProcessVolume component
                        PostProcessVolume postProcessVolume = mainCamera.GetComponent<PostProcessVolume>();
                        if (postProcessVolume != null)
                        {
                            // Toggle the enabled state of the PostProcessVolume component
                            postProcessVolume.enabled = !postProcessVolume.enabled;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draws the Add Pixy option in the mod menu
        /// </summary>
        private void UnlockGalleryButtons()
        {
            // Begin horizontal layout for the Add Pixy option
            GUILayout.BeginHorizontal();
            
            // Draw Blue Dot
            DrawBlueDot();

            if (GUILayout.Button("Unlock Gallery Buttons"))
            {
                // Check if the current scene is "Gallery"
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Gallery")
                {
                    // Debug log the action being performed
                    Debug.Log("Toggle Gallery Buttons");

                    // Find the "Button Menu" GameObject within "Gallery Canvas"
                    GameObject buttonMenu = GameObject.Find("Gallery Canvas/Button Menu");
                    if (buttonMenu != null)
                    {
                        // Find all child GameObjects within "Button Menu"
                        foreach (Transform buttonTransform in buttonMenu.transform)
                        {
                            // Get the Selectable component of each button GameObject
                            Selectable selectable = buttonTransform.GetComponent<Selectable>();
                            if (selectable != null)
                            {
                                // Set the interactability of the button to true
                                selectable.interactable = true;
                            }
                        }
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
        
        /// <summary>
        /// Draws the Glory Hole Uses option in the mod menu
        /// </summary>
        private void DrawInfiniteStaminaButton()
        {
            // Begin horizontal layout for the Glory Hole Uses option
            GUILayout.BeginHorizontal();

            // Draw the activation dot and use the Glory Hole Uses value to set its color
            DrawActivationDot(_staminaValue != 9);

            // Add a label for the text field
            GUILayout.Label("Stamina:"); // The label for the text field

            // Draw the text field and capture user input
            string inputText = GUILayout.TextField(_staminaValue.ToString(), GUILayout.Width(40)); // The text field for the Glory Hole Uses value

            // Try to parse the input text as an integer
            if (int.TryParse(inputText, out int newMaxStamina))
            {
                // Check if the new value is different from the current value
                if (newMaxStamina != _staminaValue)
                {
                    // Update the Glory Hole Uses value
                    _staminaValue = newMaxStamina;

                    // Find the "Player(Clone)" GameObject in the scene "DontDestroyOnLoad"
                    GameObject playerObject = GameObject.Find("Player(Clone)");
    
                    if (playerObject != null)
                    {
                        // Get the Stamina component attached to the Player GameObject
                        Stamina staminaComponent = playerObject.GetComponent<Stamina>();
        
                        if (staminaComponent != null)
                        {
                            // Set the currentStamina to 99
                            staminaComponent.currentStamina = newMaxStamina;

                            // Set maxStamina to a desired value
                            staminaComponent.maxStamina = newMaxStamina;
                        }
                    }
                }
            }

            // End horizontal layout for the Glory Hole Uses option
            GUILayout.EndHorizontal();
        }
        
        /// <summary>
        /// Handles button click for toggling stamina drain in the scene
        /// </summary>
        private static void ToggleStaminaDrain()
        {
            // Debug log the action being performed
            Debug.Log("Toggle Stamina Drain");

            // Find the "Player(Clone)" GameObject in the scene "DontDestroyOnLoad"
            GameObject playerObject = GameObject.Find("Player(Clone)");

            if (playerObject != null)
            {
                // Get the Stamina component attached to the Player GameObject
                Stamina staminaComponent = playerObject.GetComponent<Stamina>();

                if (staminaComponent != null)
                {
                    // Check the current value of the stamina drain rate
                    if (Mathf.Approximately(staminaComponent.staminaRegenCooldown, 0.65f))
                    {
                        // If it's 0.68, set it to 0.0001
                        staminaComponent.staminaRegenCooldown = 0.0001f;
                    }
                    else if (Mathf.Approximately(staminaComponent.staminaRegenCooldown, 0.0001f))
                    {
                        // If it's 0.0001, set it to 0.68
                        staminaComponent.staminaRegenCooldown = 0.65f;
                    }
                }
            }
        }
        
        /// <summary>
        /// Draws the Glory Hole Uses option in the mod menu
        /// </summary>
        private void DrawInfiniteHealthButton()
        {
            // Begin horizontal layout for the Glory Hole Uses option
            GUILayout.BeginHorizontal();

            // Draw the activation dot and use the Glory Hole Uses value to set its color
            DrawActivationDot(_healthValue != 5);

            // Add a label for the text field
            GUILayout.Label("Health:"); // The label for the text field

            // Draw the text field and capture user input
            string inputText = GUILayout.TextField(_healthValue.ToString(), GUILayout.Width(40)); // The text field for the Glory Hole Uses value

            // Try to parse the input text as an integer
            if (int.TryParse(inputText, out int newMaxHP))
            {
                // Check if the new value is different from the current value
                if (newMaxHP != _healthValue)
                {
                    // Update the Glory Hole Uses value
                    _healthValue = newMaxHP;

                    // Find the "Player(Clone)" GameObject in the scene "DontDestroyOnLoad"
                    GameObject playerObject = GameObject.Find("Player(Clone)");
    
                    if (playerObject != null)
                    {
                        // Get the Stamina component attached to the Player GameObject
                        Health healthComponent = playerObject.GetComponent<Health>();
        
                        if (healthComponent != null)
                        {
                            // Set the currentStamina to 99
                            healthComponent.currentHp = newMaxHP;

                            // Set maxStamina to a desired value
                            healthComponent.maxHp = newMaxHP;
                        }
                    }
                }
            }

            // End horizontal layout for the Glory Hole Uses option
            GUILayout.EndHorizontal();
        }
        
        /// <summary>
        /// Handles button click for toggling stamina drain in the scene
        /// </summary>
        private static void ToggleEnemyDamage()
        {
            // Debug log the action being performed
            Debug.Log("Toggle Enemy Damage");

            // Find the "Player(Clone)" GameObject in the scene "DontDestroyOnLoad"
            GameObject playerObject = GameObject.Find("Player(Clone)");

            if (playerObject != null)
            {
                // Get the Stamina component attached to the Player GameObject
                Health healthComponent = playerObject.GetComponent<Health>();
        
                if (healthComponent != null)
                {
                    // Set the currentStamina to 99
                    healthComponent.OnCooldown = !healthComponent.OnCooldown;
                    
                    // Log the action being performed
                    Debug.Log("Enemy damage toggled. New value: " + healthComponent.OnCooldown);
                }
            }
        }
    }
}