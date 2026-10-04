using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace KSPProfiler
{
    /// <summary>
    /// Marks a method as a tool of KSP-MCPServer (https://github.com/lhervier/KSP-MCPServer), which looks for
    /// an attribute of this name in every loaded mod. Declared here so that this mod depends on nothing: when
    /// the server is not installed, nothing reads it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class McpToolAttribute : Attribute
    {
        public McpToolAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; }
        public string Description { get; }
    }

    /// <summary>
    /// What KSP-MCPServer, when it is installed, offers of this mod as tools: the buttons and fields of its
    /// window, pressed and filled as a player does, and nothing that would measure differently.
    /// </summary>
    internal static class McpTools
    {
        [McpTool("profiler_open",
            "Opens the window of KSP Profiler, as its toolbar button does: the profiler only measures while " +
                "it is open. Does nothing when it is open already. Returns the state of the capture.")]
        internal static object Open()
        {
            if (GameLoopProfilerUI.Instance == null)
            {
                UIManager.Toggle();
            }
            return State();
        }

        [McpTool("profiler_start_capture",
            "Starts a capture of a number of frames (at most 10000), as a player does: AutoCapture turned off, " +
                "Frames to capture typed in, Start capture pressed. The capture stops by itself at that number, " +
                "or on profiler_stop_capture. Returns the state of the capture.")]
        internal static object StartCapture(double frames)
        {
            GameLoopProfilerUI ui = Window();
            if (GameLoopProfiler.autoCapture)
            {
                ui.PressAutoCapture();
            }
            ui.TypeCapturedFrames((int)frames);
            if (!GameLoopProfiler.captureEnabled)
            {
                ui.PressManualCapture();
            }
            return State();
        }

        [McpTool("profiler_stop_capture",
            "Stops the capture in progress, as Stop capture does. Returns the state of the capture.")]
        internal static object StopCapture()
        {
            GameLoopProfilerUI ui = Window();
            if (!GameLoopProfiler.autoCapture && GameLoopProfiler.captureEnabled)
            {
                ui.PressManualCapture();
            }
            return State();
        }

        [McpTool("profiler_export",
            "Exports the statistics shown to a CSV file, as Export to CSV does, into a folder (created when " +
                "missing) under a file name. Returns the path written.")]
        internal static object Export(string directory, string fileName)
        {
            GameLoopProfilerUI ui = Window();
            Directory.CreateDirectory(directory);
            ui.PressExport(directory, fileName);
            return Path.Combine(directory, fileName);
        }

        [McpTool("profiler_move_window",
            "Moves the window of KSP Profiler, as dragging it does: x and y of the window's centre from the " +
                "screen's centre, in the units of the game's interface, y upwards. Returns its position and size " +
                "(x, y, width, height).")]
        internal static object MoveWindow(double x, double y)
        {
            Window();
            RectTransform transform = UIManager.window.TopTransform;
            transform.anchoredPosition = new Vector2((float)x, (float)y);
            return new Dictionary<string, object>
            {
                { "x", (double)transform.anchoredPosition.x },
                { "y", (double)transform.anchoredPosition.y },
                { "width", (double)transform.rect.width },
                { "height", (double)transform.rect.height }
            };
        }

        [McpTool("profiler_state",
            "Reads the state of the capture of KSP Profiler: whether its window is open, AutoCapture, whether " +
                "a capture is running, the frames captured and the frames asked for.")]
        internal static object State()
        {
            return new Dictionary<string, object>
            {
                { "open", GameLoopProfilerUI.Instance != null },
                { "autoCapture", GameLoopProfiler.autoCapture },
                { "capturing", GameLoopProfiler.captureEnabled },
                { "capturedFrames", GameLoopProfiler.capturedFrames },
                { "framesToCapture", GameLoopProfiler.maxCapturedFrames }
            };
        }

        private static GameLoopProfilerUI Window()
        {
            if (GameLoopProfilerUI.Instance == null)
            {
                throw new InvalidOperationException("The window of KSP Profiler is closed: profiler_open first");
            }
            return GameLoopProfilerUI.Instance;
        }
    }
}
