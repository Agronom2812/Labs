using Multithreading;

int completedThreads = 0;
object consoleLock = new object();

FunctionSolver solver = new FunctionSolver();

int lastProgress = -1;

solver.ProgressChanged += (threadId, progress) => {
    lock (consoleLock) {
        if (progress == lastProgress) return;

        lastProgress = progress;

        Console.SetCursorPosition(0, threadId);

        Console.Write($"Поток {threadId} : [");
        int barLength = progress / 2;
        barLength = Math.Clamp(barLength, 0, 50);
        Console.Write(new string('=', barLength));
        Console.Write(new string(' ', 50 - barLength));
        Console.Write($"] {progress}%");
    }
};

solver.CalculationCompleted += (result, elapsedTicks) => {
    lock (consoleLock) {

        Interlocked.Increment(ref completedThreads);

        Console.SetCursorPosition(0, 6 + completedThreads * 2);
        Console.WriteLine($"Поток {Environment.CurrentManagedThreadId} : Завершился с результатом: {result}");
        Console.WriteLine($"Время выполнения (тики): {elapsedTicks}");
    }
};

for (int i = 0; i < 5; i++) {
    int lineOffset = i % 2;
    Thread thread = new Thread(() => solver.RectangleMethod(lineOffset));
    thread.Start();
    thread.Join();
}
Console.SetCursorPosition(0, 20);


Console.WriteLine($"===============> End of thread {Thread.CurrentThread.ManagedThreadId}");
