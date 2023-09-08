using System;
using Chess;
using Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GUI;

public class RobeGUI : Game
{
    private int PieceSize;
    private Texture2D wKing;
    private GraphicsDeviceManager Graphics;
    private SpriteBatch sBatch;

    public RobeGUI()
    {
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        PieceSize = Konfig.ChessSquareSize;

        Graphics.PreferredBackBufferWidth = 1080;
        Graphics.PreferredBackBufferHeight = 720;

        Graphics.ApplyChanges();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        sBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
        Texture2D wking = Content.Load<Texture2D>(Konfig.ChessPiecesPack + "9");
        wKing = new Texture2D(GraphicsDevice, PieceSize, PieceSize);

        Color[] data = new Color[wking.Width * wking.Height];
        wking.GetData(data);

        GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

        Color[] newData = new Color[PieceSize * PieceSize];
        for (int y = 0; y < PieceSize; y++)
        {
            for (int x = 0; x < PieceSize; x++)
            {
                int sourceX = x * wking.Width / PieceSize;
                int sourceY = y * wking.Height / PieceSize;

                newData[x + y * PieceSize] = data[sourceX + sourceY * wking.Width];
            }
        }

        wKing.SetData(newData);

    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here
        KeyboardState keystate = Keyboard.GetState();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here
        sBatch.Begin();
        sBatch.Draw(wKing, new Rectangle(0, 0, PieceSize, PieceSize), Color.White);
        sBatch.End();

        base.Draw(gameTime);
    }
}
