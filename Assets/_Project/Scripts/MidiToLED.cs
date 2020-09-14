using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NAudio.Midi;
using SerialStrings = Helios.Settings.Strings.Serial;
using MidiStrings = Helios.Settings.Strings.Midi;

namespace MMM.LED
{
    public class FadeNote
    {
        public bool fadingIn;
        public float value;

        public FadeNote(bool isFadingIn = true)
        {
            fadingIn = isFadingIn;
            value = isFadingIn ? 0 : 1;
        }
    }
    public class MidiToLED : MonoBehaviour
    {
        [SerializeField] private bool _printDevices = true;
        [SerializeField] private LEDSerialController _lights;
        private Queue<MidiEvent> _midiEvents;
        private MidiIn _midiIn;
        private Vector2Int _midiRange;
        private Vector2Int _lightRow1;
        private Vector2Int _lightRow2;
        private Helios.Settings.Group _midiSettings = Helios.Settings.Group.Get(MidiStrings.GroupName);
        private Helios.Settings.Group _lightSettings = Helios.Settings.Group.Get(SerialStrings.GroupName);
        private Dictionary<int, FadeNote> _fadeNotes;
        
        void Start()
        {
            if (_printDevices) PrintDevices();
            
            int deviceIndex = _midiSettings.Get<int>(MidiStrings.DeviceIndex);
            _midiIn = new MidiIn(deviceIndex);
            if (_midiIn != null)
            {
                Debug.Log($"Connected to {MidiIn.DeviceInfo(deviceIndex).ProductName}");
                SetupMidiInput();
                SetupLightRanges();
            }
            else
            {
                Debug.LogError($"Couldn't open device at {deviceIndex}.");
            }
        }

        private void SetupMidiInput()
        {
            _midiEvents = new Queue<MidiEvent>();
            _fadeNotes = new Dictionary<int, FadeNote>();
            
            _midiRange.x = _midiSettings.Get<int>(MidiStrings.RangeLow);
            _midiRange.y = _midiSettings.Get<int>(MidiStrings.RangeHigh);
            
            _midiIn.MessageReceived += OnMessageReceived;
            _midiIn.Start();
        }

        private void SetupLightRanges()
        {
            _lightRow1.x = _lightSettings.Get<int>(SerialStrings.Range1Low);
            _lightRow1.y = _lightSettings.Get<int>(SerialStrings.Range1High);
            _lightRow2.x = _lightSettings.Get<int>(SerialStrings.Range2Low);
            _lightRow2.y = _lightSettings.Get<int>(SerialStrings.Range2High);
        }

        private void PrintDevices()
        {
            for (int i = 0; i < MidiIn.NumberOfDevices; i++)
            {
                var info = MidiIn.DeviceInfo(i);
                Debug.Log($"{i}: {info.ProductName}");
            }
        }

        private void OnMessageReceived(object sender, MidiInMessageEventArgs e)
        {
            _midiEvents.Enqueue(e.MidiEvent);
        }

        private void ProcessEvent(MidiEvent e)
        {
            switch (e.CommandCode)
            {
                case MidiCommandCode.NoteOn:
                    FadeNoteOn((NoteOnEvent) e);
                    break;
                case MidiCommandCode.NoteOff:
                    FadeNoteOff((NoteEvent) e);
                    break;
            }
        }

        private void NoteOn(NoteOnEvent e)
        {
            // Remove from current fades
            if (_fadeNotes.ContainsKey(e.NoteNumber)) _fadeNotes.Remove(e.NoteNumber);
            
            var lights = LightsForNote(e.NoteNumber);
            var color = ColorForNote(e.NoteNumber);
            for (int i = 0; i < lights.Length; i++)
            {
                _lights.SetSingleLight(lights[i], color);   
            }
            _lights.Send();
        }
        
        private void FadeNoteOn(NoteOnEvent e)
        {
            if (_fadeNotes.ContainsKey(e.NoteNumber))
            {
                // already exists, reset it
                _fadeNotes[e.NoteNumber].fadingIn = true;
            }
            else
            {
                _fadeNotes.Add(e.NoteNumber, new FadeNote(true));
            }
        }

        private Color32 ColorForNote(int noteNumber)
        {
            
            int key = noteNumber % 12;
            /*
            switch (key)
            {
                case 0: return new Color(255, 0, 0);
                case 1: return new Color(144, 0, 255);
                case 2: return new Color(255, 255, 0);
                case 3: return new Color(144, 0, 255);
                case 4: return new Color(195, 242, 255);
                case 5: return new Color(171, 0, 52);
                case 6: return new Color(127, 139, 253);
                case 7: return new Color(255, 127, 0);
                case 8: return new Color(187, 117, 252);
                case 9: return new Color(51, 204, 51);
                case 10: return new Color(169, 103, 124);
                case 11: return new Color(142, 201, 255);
                default: return Color.black;
            }
            */
            float normalizedNote = (float) key / 12f;
            var color = Color.HSVToRGB(normalizedNote, 1, 1f);
            return new Color32( 
                Convert.ToByte(color.r * 255), 
                Convert.ToByte(color.g * 255), 
                Convert.ToByte(color.b * 255), 
                255);
        }
        
        private void FadeNoteOff(NoteEvent e)
        {
            if (_fadeNotes.ContainsKey(e.NoteNumber))
            {
                // already exists, reset it
                _fadeNotes[e.NoteNumber].fadingIn = false;
            }
            else
            {
                _fadeNotes.Add(e.NoteNumber, new FadeNote(false));
            }
        }

        private void NoteOff(NoteEvent e)
        {
            var lights = LightsForNote(e.NoteNumber);
            for (int i = 0; i < lights.Length; i++)
            {
                _lights.SetSingleLight(lights[i], Color.black);   
            }
            _lights.Send();
        }

        private int[] LightsForNote(int note)
        {
            float normalizedNote = Mathf.InverseLerp(_midiRange.x, _midiRange.y, note);
            
            int row1Light = Mathf.RoundToInt(
                Mathf.Lerp(_lightRow1.x, _lightRow1.y, normalizedNote));
            
            int row2Light = Mathf.RoundToInt(
                Mathf.Lerp(_lightRow2.x, _lightRow2.y, normalizedNote));
            
            return new int[]{row1Light, row2Light};
        }

        private void Update()
        {
            while (_midiEvents.Count > 0)
            {
                ProcessEvent(_midiEvents.Dequeue());
            }
            
            // handle fading notes
            if (_fadeNotes.Count > 0)
            {
                List<int> notesToRemove = new List<int>();
                foreach (KeyValuePair<int,FadeNote> valuePair in _fadeNotes.ToArray())
                {
                    var key = valuePair.Key;
                    var fadeNote = valuePair.Value;
                    var currentValue = fadeNote.value;
                    var newValue = (fadeNote.fadingIn) ? currentValue + _midiSettings.Get<float>(MidiStrings.FadeInSpeed) : currentValue - _midiSettings.Get<float>(MidiStrings.FadeOutSpeed);
                    
                    var originalColor = ColorForNote(key);
                    var newColor = Color32.Lerp(originalColor, Color.black, 1 - newValue);
                    foreach (int lightIndex in LightsForNote(key))
                    {
                        _lights.SetSingleLight(lightIndex, newColor);
                    }

                    if ((!fadeNote.fadingIn && newValue <= 0) || fadeNote.fadingIn && newValue >= 1)
                    {
                        notesToRemove.Add(key);
                    }
                    else
                    {
                        _fadeNotes[key].value = newValue;   
                    }
                }
                _lights.Send();

                // remove keys that are done
                foreach (int i in notesToRemove)
                {
                    _fadeNotes.Remove(i);
                }
            }
        }

        private void OnDestroy()
        {
            if (_midiIn != null)
            {
                _midiIn.MessageReceived -= OnMessageReceived;
                _midiIn.Close();
            }
        }
    }

}