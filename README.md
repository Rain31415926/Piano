# 電子琴

這是一個功能豐富的 **C# Windows Forms** 電子琴專案。除了基本的滑鼠互動彈奏外，本專案更引入了**非同步自動演奏系統**，能夠在軟體介面上模擬真人彈奏，並即時顯示琴鍵反饋。

## 🚀 核心功能

- **非同步自動演奏**：
  - 內建經典曲目《小星星》。
  - 使用 `async/await` 與 `Task.Delay` 技術，確保演奏時 UI 介面不會卡死，維持流暢體驗。
- **即時視覺回饋 **：
  - 自動演奏時，對應的琴鍵會變色（白鍵變灰、黑鍵變暗灰），模擬按下效果。
  - 使用 `Dictionary` 高效管理琴鍵物件與原始顏色，實現精確的視覺還原。
- **專業 MIDI 音效**：調用 `winmm.dll` 直接驅動系統合成器，延遲極低。
- **全音域配置**：覆蓋 3 個八度 (C3 - B5)，並清晰標註「中央 Do」。

## 🖥️ 使用畫面展示

<img width="1067" height="382" alt="image" src="https://github.com/user-attachments/assets/4b31ae6d-9529-4258-a8e0-a05f7cedfa46" />

### 🎼 操作指南
1. **手動彈奏**：直接點擊畫面上的黑白鍵即可發聲。
2. **自動演奏**：點擊左下角的「自動演奏：小星星」，程式將自動依序彈奏旋律，同時琴鍵會伴隨節奏閃爍。

## 🛠️ 技術實作細節

### 1. 非同步演奏演算法
為了在不阻塞主執行緒（UI Thread）的情況下播放音樂，專案採用了 `Task.Delay`：
```csharp
for (int i = 0; i < melody.Length; i++)
{
    // 發送 MIDI 按下指令
    midiOutShortMsg(midiOutHandle, msgOn);
    
    // 視覺回饋：改變琴鍵顏色
    keyBtn.BackColor = Color.Gray; 

    await Task.Delay(durations[i] - 50); // 依節拍等待

    // 發送 MIDI 放開指令與還原顏色
    midiOutShortMsg(midiOutHandle, msgOff);
    keyBtn.BackColor = originalColors[keyBtn];

    await Task.Delay(50); // 音符間短暫停頓，增加層次感
}
