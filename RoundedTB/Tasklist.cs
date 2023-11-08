using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UIAutomationClient;
//PowerToys Shortcut-Guide code ported to C#

public class Tasklist
{
    private IUIAutomation automation;
    private IUIAutomationElement element;

    private int UIA_BoundingRectanglePropertyId = 30001;//Unused

    public void Update()
    {
        IntPtr tasklistHwnd = FindTasklistWindow();

        if (tasklistHwnd == IntPtr.Zero)
            return;

        InitializeAutomation();

        element = automation.ElementFromHandle(tasklistHwnd);
    }

    public bool UpdateButtons(List<TasklistButton> buttons)
    {
        if (automation == null || element == null)
            return false;

        IUIAutomationElementArray elements = element.FindAll(TreeScope.TreeScope_Children, automation.CreateTrueCondition());

        if (elements == null)
            return false;

        int count = elements.Length;

        List<TasklistButton> foundButtons = new List<TasklistButton>();
        foundButtons.Capacity = count;

        for (int i = 0; i < count; ++i)
        {
            IUIAutomationElement child = elements.GetElement(i);

            TasklistButton button = new TasklistButton();
            object varRect = child.GetCurrentPropertyValue(30001); //child.GetCurrentPropertyValue(UIA_BoundingRectanglePropertyId);
            if (varRect is double[] rectArray && rectArray.Length == 4)
            {
                button.x = (int)rectArray[0];
                button.y = (int)rectArray[1];
                button.width = (int)rectArray[2];
                button.height = (int)rectArray[3];
            }
            else
            {
                return false;
            }
            string automationId = child.GetCurrentPropertyValue(30011); //UIA_AutomationIdPropertyId 
            button.name = automationId;

            foundButtons.Add(button);
        }

        buttons.Clear();
        for (int i = 0; i < foundButtons.Count; i++)
        {
            if (buttons.Count == 0)
            {
                foundButtons[i].keynum = 1;
                buttons.Add(foundButtons[i]);
            }
            else
            {
                if (foundButtons[i].x < buttons[buttons.Count - 1].x || foundButtons[i].y < buttons[buttons.Count - 1].y)
                    break;
                if (foundButtons[i].name == buttons[buttons.Count - 1].name)
                    continue;

                foundButtons[i].keynum = buttons[buttons.Count - 1].keynum + 1;
                buttons.Add(foundButtons[i]);

                if (buttons[buttons.Count - 1].keynum == 10)
                    break;
            }
        }
        return true;
    }

    public List<TasklistButton> GetButtons()
    {

        List<TasklistButton> buttons = new List<TasklistButton>();
        UpdateButtons(buttons);
        return buttons;
    }

    private IntPtr FindTasklistWindow()
    {
        IntPtr tasklistHwnd = FindWindow("Shell_TrayWnd", null);
        if (tasklistHwnd == IntPtr.Zero)
            return IntPtr.Zero;

        tasklistHwnd = FindWindowEx(tasklistHwnd, IntPtr.Zero, "ReBarWindow32", null);
        if (tasklistHwnd == IntPtr.Zero)
            return IntPtr.Zero;

        tasklistHwnd = FindWindowEx(tasklistHwnd, IntPtr.Zero, "MSTaskSwWClass", null);
        if (tasklistHwnd == IntPtr.Zero)
            return IntPtr.Zero;

        tasklistHwnd = FindWindowEx(tasklistHwnd, IntPtr.Zero, "MSTaskListWClass", null);
        return tasklistHwnd;
    }

    private void InitializeAutomation()
    {
        if (automation == null)
        {
            automation = new CUIAutomation();
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

    public class TasklistButton
    {
        public string? name;
        public int x;
        public int y;
        public int width;
        public int height;
        public int keynum;
    }
}
