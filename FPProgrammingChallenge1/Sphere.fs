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

module Shapes

open System
open OpenTK
open OpenTK.Graphics.OpenGL

module ExtraMath=
    let calcSphere(rho, phi, theta) =
        let x = rho * sin(float32 phi) * cos(theta)
        let y = rho * cos(phi)
        let z = rho * sin(phi) * sin(theta)
        new Vector3(x, y, z)

type Sphere(x, y, z, r, g, b) =

    //variables
    let mutable vertVBO = 0
    let mutable normVBO = 0
    let mutable indexVBO = 0

    let mutable indicesLength = 0

    let mutable position = new Vector3(x, y, z)
    let mutable color = new Vector3(r, g, b)

    //private methods
    let InitSphereCoords() =

        let vertices = [|   for phi in 0.0f .. float32 System.Math.PI / 7.0f .. float32 System.Math.PI do
                                for theta in 0.0f .. float32 System.Math.PI / 4.0f .. 2.0f * float32 System.Math.PI do
                                    yield ExtraMath.calcSphere(1.0f, phi, theta) |]

        let normals = [| for vec in vertices do
                            yield Vector3.Normalize(vec) |]

        let indices2D = [| for vertex = 1 to vertices.Length - 8 do
                            let tl = uint32 vertex
                            let bl = uint32 vertex + 8u
                            let br = if (vertex <> 1 && vertex - 1 % 8 = 0) then uint32 vertex + 1u else uint32 vertex + 9u
                            let tr = if (vertex <> 1 && vertex - 1 % 8 = 0) then uint32 vertex - 7u else uint32 vertex + 1u

                            yield [| tl; bl; br; tl; br; tr |] |]

        let indices = [| for quad in indices2D do
                            for value in quad do
                                yield value |]
        
        indicesLength <- indices.Length

        //generate buffer handles
        vertVBO <- GL.GenBuffers(1)
        normVBO <- GL.GenBuffers(1)
        indexVBO <- GL.GenBuffers(1)

        //send vertices to GPU
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertVBO)
        GL.BufferData(BufferTarget.ArrayBuffer, nativeint(vertices.Length * Vector3.SizeInBytes), vertices, BufferUsageHint.StaticDraw)

        //send normals to GPU
        GL.BindBuffer(BufferTarget.ArrayBuffer, normVBO)
        GL.BufferData(BufferTarget.ArrayBuffer, nativeint(normals.Length * Vector3.SizeInBytes), normals, BufferUsageHint.StaticDraw)
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0)

        //send indices to GPU
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexVBO)
        GL.BufferData(BufferTarget.ElementArrayBuffer, nativeint(indices.Length * sizeof<float32>), indices, BufferUsageHint.StaticDraw)
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0)

        ()

    //constructor method calls
    do InitSphereCoords()

    //public methods
    member this.Draw() =

        //backface culling
        //GL.Enable EnableCap.CullFace
        //GL.FrontFace(FrontFaceDirection.Ccw)
        //GL.CullFace CullFaceMode.Back

        //settings for this sphere
        GL.Translate position
        GL.Color3(color)
        
        //bind the vertices
        GL.EnableClientState ArrayCap.VertexArray
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertVBO)
        GL.VertexPointer(3, VertexPointerType.Float, 0, nativeint(0))
        
        //bind the normals
        GL.EnableClientState ArrayCap.NormalArray
        GL.BindBuffer(BufferTarget.ArrayBuffer, normVBO)
        GL.NormalPointer(NormalPointerType.Float, 0, nativeint(0))
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0)

        //bind the indices, draw the sphere
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexVBO)
        GL.DrawElements(BeginMode.Triangles, indicesLength, DrawElementsType.UnsignedInt, nativeint(0))
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0)

        //disable stuff
        //GL.Disable EnableCap.CullFace
        GL.DisableClientState ArrayCap.VertexArray
        GL.DisableClientState ArrayCap.NormalArray
        ()

    member this.Unload() =
        GL.DeleteBuffers(1, ref vertVBO)
        GL.DeleteBuffers(1, ref normVBO)
        GL.DeleteBuffers(1, ref indexVBO)

        vertVBO <- 0
        normVBO <- 0
        indexVBO <- 0
        ()

    //properties
    member this.Position with get () = position and set (value:Vector3) = position <- value
    member this.Color with get () = color and set (value:Vector3) = color <- value

    //constructor overloads
    new() = Sphere(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f)
    new(r, g, b) = Sphere(0.0f, 0.0f, 0.0f, r, g, b)
    new(pos:Vector3, color:Vector3) = Sphere(pos.X, pos.Y, pos.Z, color.X, color.Y, color.Z)