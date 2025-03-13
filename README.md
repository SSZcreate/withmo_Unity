# withmo (Unity Side)

**withmo** は、スマートフォンのホーム画面に 3D キャラクターを表示し、インタラクションを楽しめる Android 向けカスタマイズアプリです。本リポジトリは Unity 側のプロジェクトです。

## 🚀 Features

- **3Dアバターの表示**: ホーム画面に好きな VRM アバターを設定
- **キャラクターとのインタラクション**:
  - タップで反応
  - 長押しで指を合わせる
  - 待機モーションの自動再生
- **ホーム画面のカスタマイズ**:
  - アバターサイズ調整
  - 背景テーマ変更（時間帯ごとの自動切り替え対応）
  - 通知時のエフェクト表示

## 📸 Video
  https://youtu.be/LpHIyaCpmzE
## 🛠 Tech Stack

### **Unity (3Dキャラクター制御)**
- **開発言語**: C#
- **使用ライブラリ**:
  - UniVRM10 (VRM アバター対応)
  - DOTween (アニメーション制御)
  - Animation Rigging (骨格アニメーション)
- **シェーダー**:
  - Shader Graph (マテリアル表現)
- **開発環境**:
  - Unity 2022.3.10f1
  - URP (Universal Render Pipeline)

## 📦 Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/withmo-unity.git
   cd withmo-unity
   ```
2. **Setup Unity**
   - Unity Hub で `Unity 2022.3.10f1` をインストール
   - `withmo-unity` プロジェクトを開く
3. **Import Dependencies**
   - `Packages/manifest.json` を開き、必要なライブラリが導入されていることを確認
   - `UniVRM`, `DOTween`, `Animation Rigging` などのライブラリが不足している場合は導入
4. **Build & Run**
   - `File -> Build Settings` で `Android` を選択
   - `Build` または `Build and Run` を実行

## 🛠 Development

### **Lint & Code Quality**
- コード品質チェックに `Rider`, `Visual Studio` の `Analyzer` を使用
- `GitHub Actions` による CI/CD パイプラインを構築予定

### **Issue Management**
- バグ報告・機能要望は `Issues` タブからお願いします。

## 💡 Roadmap
- [ ] バッテリー消費量の最適化
- [ ] キャラクターのモーション追加
- [ ] より直感的なカスタマイズUI

## 🤝 Contributing

Pull Request は大歓迎です！バグ修正や新機能の提案があれば、Issue を作成してください。

1. **Fork** このリポジトリ
2. **新しいブランチを作成** (`git checkout -b feature-branch`)
3. **変更をコミット** (`git commit -m 'Add new feature'`)
4. **プッシュ** (`git push origin feature-branch`)
5. **Pull Request を作成**

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](./LICENSE) file for details.

## 👨‍💻 Author

Developed by **少年少女浪漫研**

X: [@SSZcreate](https://x.com/SSZcreate) 
