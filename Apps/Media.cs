using System.Runtime.InteropServices;
using WindowsInput.Native;
using WindowsInput;

namespace HoloPad.Apps
{
    internal class MediaApp
    {
        
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);
        const int KEYEVENTF_EXTENTEDKEY = 1;
        const int KEYEVENTF_KEYUP = 0;

        //https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
        const int VK_MEDIA_NEXT_TRACK = 0xB0;// code to jump to next track
        const int VK_MEDIA_PLAY_PAUSE = 0xB3;// code to play or pause a song
        const int VK_MEDIA_PREV_TRACK = 0xB1;// code to jump to prev track
        const int CTRL = 0x11;
        const int SHIFT = 0xA0;
        const int ALT = 0x12;
        const int P = 0x50;

        InputSimulator inputSim;

        public MediaApp() => inputSim = new InputSimulator();

        public void ToggleDiscordMute()
        {
            //https://stackoverflow.com/questions/3047375/simulating-key-press-c-sharp

            //https://ourcodeworld.com/articles/read/520/simulating-keypress-in-the-right-way-using-inputsimulator-with-csharp-in-winforms
            /*inputSim.Keyboard.ModifiedKeyStroke(
                new[] { VirtualKeyCode.CONTROL, VirtualKeyCode.MENU, VirtualKeyCode.SHIFT },
                new[] { VirtualKeyCode.VK_P }
            );*/

            inputSim.Keyboard.KeyPress(VirtualKeyCode.F8);
        }
        public void AudioPlayPause() => inputSim.Keyboard.KeyPress(VirtualKeyCode.MEDIA_PLAY_PAUSE);// keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
        public void AudioNext() => inputSim.Keyboard.KeyPress(VirtualKeyCode.MEDIA_NEXT_TRACK);//keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
        public void AudioPrev() => inputSim.Keyboard.KeyPress(VirtualKeyCode.MEDIA_PREV_TRACK);//keybd_event(VK_MEDIA_PREV_TRACK, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
    }
}
