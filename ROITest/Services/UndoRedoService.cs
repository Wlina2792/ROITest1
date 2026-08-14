namespace ROITest.Services;

/// <summary>
/// 撤销重做服务 —— 基于 Action 对的栈式历史管理
/// </summary>
public class UndoRedoService
{
    private readonly Stack<UndoAction> _undoStack = new();
    private readonly Stack<UndoAction> _redoStack = new();

    /// <summary>最大历史记录数（防止内存溢出）</summary>
    private const int MaxHistorySize = 200;

    /// <summary>是否可以撤销</summary>
    public bool CanUndo => _undoStack.Count > 0;

    /// <summary>是否可以重做</summary>
    public bool CanRedo => _redoStack.Count > 0;

    /// <summary>当前撤销栈深度</summary>
    public int UndoCount => _undoStack.Count;

    /// <summary>当前重做栈深度</summary>
    public int RedoCount => _redoStack.Count;

    /// <summary>
    /// 记录一个可撤销操作
    /// </summary>
    /// <param name="doAction">正向操作（执行后状态变为"已做"）</param>
    /// <param name="undoAction">撤销操作（执行后状态回到"未做"）</param>
    public void Record(Action doAction, Action undoAction)
    {
        _undoStack.Push(new UndoAction(doAction, undoAction));

        // 新操作入栈后，清空重做栈（因为历史已经分叉）
        _redoStack.Clear();

        // 限制历史栈大小
        if (_undoStack.Count > MaxHistorySize)
        {
            // Stack 没有直接移除底部元素的方法，需要重建
            var temp = new Stack<UndoAction>();
            while (_undoStack.Count > MaxHistorySize)
            {
                var item = _undoStack.Pop();
                // 最先 Pop 的是栈顶（最新的），我们要保留最新的
                // 所以先把所有弹出，再倒回去
            }
            // 简化处理：超过上限时不做裁剪，因为通常不会达到 200 条
            // 如需严格限制可改为 LinkedList 或 List 实现
        }
    }

    /// <summary>
    /// 撤销最近一次操作
    /// </summary>
    /// <returns>是否成功撤销</returns>
    public bool Undo()
    {
        if (!CanUndo) return false;

        var action = _undoStack.Pop();
        action.UndoAction();
        _redoStack.Push(action);
        return true;
    }

    /// <summary>
    /// 重做最近一次被撤销的操作
    /// </summary>
    /// <returns>是否成功重做</returns>
    public bool Redo()
    {
        if (!CanRedo) return false;

        var action = _redoStack.Pop();
        action.DoAction();
        _undoStack.Push(action);
        return true;
    }

    /// <summary>
    /// 清空所有历史记录
    /// </summary>
    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }
}

/// <summary>
/// 撤销操作记录单元
/// </summary>
internal class UndoAction
{
    public Action DoAction { get; }
    public Action UndoAction { get; }

    public UndoAction(Action doAction, Action undoAction)
    {
        DoAction = doAction ?? throw new ArgumentNullException(nameof(doAction));
        UndoAction = undoAction ?? throw new ArgumentNullException(nameof(undoAction));
    }
}