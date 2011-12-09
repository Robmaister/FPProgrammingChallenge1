(*Copyright (c) 2011 Robert Rouhani

This software is provided 'as-is', without any express or implied
warranty. In no event will the authors be held liable for any damages
arising from the use of this software.

Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:

   1. The origin of this software must not be misrepresented; you must not
   claim that you wrote the original software. If you use this software
   in a product, an acknowledgment in the product documentation would be
   appreciated but is not required.

   2. Altered source versions must be plainly marked as such, and must not be
   misrepresented as being the original software.

   3. This notice may not be removed or altered from any source
   distribution.
 
Robert Rouhani <robert.rouhani@gmail.com>*)

module Program

open System
open System.Drawing
open System.Collections.Generic

open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL
open OpenTK.Input

let ambLight = new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
let diffLight = new Vector4(0.5f, 0.5f, 0.5f, 0.0f)
let specLight = new Vector4(0.4f, 0.4f, 0.4f, 0.0f)
let posLight = new Vector4(3.0f, 3.0f, 2.0f, 0.0f)

type Game() =
    inherit GameWindow(800, 600, GraphicsMode.Default, "")

    let s = new Shapes.Sphere(0.0f, 0.0f, -2.0f, 1.0f, 1.0f, 0.0f)
    let c = new Camera.Camera(new Vector3(0.0f, 0.0f, -5.0f))
    do c.RotRightAxis(0.0f);

    do base.VSync <- VSyncMode.On

    override o.OnLoad e =
        base.OnLoad e
        GL.ClearColor (0.39f, 0.58f, 0.93f, 1.0f)
        GL.Enable EnableCap.DepthTest
        GL.Enable EnableCap.Lighting
        GL.Enable EnableCap.Light0

    override o.OnUpdateFrame e =
        base.OnUpdateFrame e
        do base.Title <- "FP Programming Challenge #1 FPS: " + int32(1.0 / e.Time).ToString()

        //handle keyboard input
        if base.Keyboard.[Key.W] then c.Move(0.08f)
        if base.Keyboard.[Key.A] then c.Strafe(-0.08f)
        if base.Keyboard.[Key.S] then c.Move(-0.08f)
        if base.Keyboard.[Key.D] then c.Strafe(0.08f)

        if base.Keyboard.[Key.Up] then c.RotUpAxis(-2.0f)
        if base.Keyboard.[Key.Left] then c.RotRightAxis(2.0f)
        if base.Keyboard.[Key.Down] then c.RotUpAxis(2.0f)
        if base.Keyboard.[Key.Right] then c.RotRightAxis(-2.0f)

        if base.Keyboard.[Key.Escape] then base.Close()

    override o.OnRenderFrame e =
        base.OnRenderFrame e

        GL.LoadMatrix(ref c.ViewMatrix)

        GL.Clear(ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)

        //lighting
        GL.Light(LightName.Light0, LightParameter.Ambient, ambLight)
        GL.Light(LightName.Light0, LightParameter.Diffuse, diffLight)
        GL.Light(LightName.Light0, LightParameter.Specular, specLight)
        GL.Light(LightName.Light0, LightParameter.Position, posLight)
        GL.Light(LightName.Light0, LightParameter.QuadraticAttenuation, 0.000000008f)

        GL.PushMatrix()
        s.Draw()
        GL.PopMatrix()

        base.SwapBuffers()

    override o.OnResize e =
        base.OnResize e

        GL.Viewport(0, 0, base.Width, base.Height)

        GL.MatrixMode MatrixMode.Projection
        GL.LoadIdentity()
        let aspect = float32 base.Width / float32 base.Height
        let projMatrix = Matrix4.CreatePerspectiveFieldOfView(float32 System.Math.PI / 4.0f * aspect, aspect, 0.1f, 100.0f)

        GL.LoadMatrix(ref projMatrix)
        GL.MatrixMode MatrixMode.Modelview
        GL.LoadIdentity()

    override o.OnUnload e =
        base.OnUnload e
        s.Unload()

[<EntryPoint>]
let main args =
    let game = new Game()
    do game.Run()
    0