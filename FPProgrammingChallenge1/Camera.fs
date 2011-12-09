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

module Camera

open System

open OpenTK

//precalculated (pi/180) and (180/pi)
let Deg2Rad = 0.01745329f
let Rad2Deg = 57.29578f

type Camera(position:Vector3) =
    let mutable pos = position

    let mutable upAxis = Vector3.UnitY
    let mutable rightAxis = Vector3.UnitX
    let mutable lookAxis = Vector3.UnitZ

    let mutable heading = 0.0f
    let mutable pitch = 0.0f

    let mutable view = Matrix4.Identity

    let UpdateViewPosition() =
        view.M41 <- -Vector3.Dot(rightAxis, pos)
        view.M42 <- -Vector3.Dot(upAxis, pos)
        view.M43 <- Vector3.Dot(lookAxis, pos)
        ()

    let RebuildView() =
        view.M11 <- rightAxis.X
        view.M12 <- upAxis.X
        view.M13 <- -lookAxis.X

        view.M21 <- rightAxis.Y
        view.M22 <- upAxis.Y
        view.M23 <- -lookAxis.Y
        
        view.M31 <- rightAxis.Z
        view.M32 <- upAxis.Z
        view.M33 <- -lookAxis.Z

        UpdateViewPosition()
        ()

    member this.Move(distance:float32) =
        pos <- pos + (distance * lookAxis)
        UpdateViewPosition()
        ()

    member this.Strafe(distance:float32) =
        pos <- pos + (distance * rightAxis)
        UpdateViewPosition()
        ()

    member this.Elevate(distance:float32) =
        pos <- pos + (distance * upAxis)
        UpdateViewPosition()
        ()

    member this.RotUpAxisTo(angle:float32) =
        heading <- angle

        if (heading >= 90.0f) then heading <- 90.0f
        if (heading <= -90.0f) then heading <- -90.0f

        let headingRadians = heading * Deg2Rad
        let localRadius = sin headingRadians

        upAxis.Y <- cos headingRadians

        let headingDirection = (pitch + 90.0f) * Deg2Rad
        upAxis.X <- localRadius * cos headingDirection
        upAxis.Z <- localRadius * sin headingDirection

        lookAxis <- Vector3.Normalize(Vector3.Cross(rightAxis, upAxis))

        RebuildView()
        ()

    member this.RotUpAxis(angle:float32) =
        this.RotUpAxisTo(heading + angle)
        ()

    member this.RotRightAxisTo(angle:float32) =
        pitch <- angle

        //clamp pitch 0 < pitch <= 360
        if (pitch >= 360.0f) then pitch <- pitch % 360.0f
        if (pitch < 0.0f) then pitch <- 360.0f + (pitch % -360.0f)

        let pitchRadians = pitch * Deg2Rad
        rightAxis.X <- cos pitchRadians
        rightAxis.Z <- sin pitchRadians

        //use this to prevent accidental camera rolling
        this.RotUpAxis(0.0f)

        lookAxis <- Vector3.Normalize(Vector3.Cross(rightAxis, upAxis))

        RebuildView()
        ()

    member this.RotRightAxis(angle:float32) =
        this.RotRightAxisTo(pitch + angle)
        ()

    member this.LookAt(point:Vector3) =
        let pitchVec = Vector2.Normalize(new Vector2(point.X - pos.X, point.Z - pos.Z))

        let y = pitchVec.Y
        let x = pitchVec.X
        this.RotRightAxisTo(Rad2Deg * atan2 y x - 90.0f)

        lookAxis <- Vector3.Normalize(point - pos)

        this.RotUpAxisTo(asin(-lookAxis.Y) * Rad2Deg)

        RebuildView()
        ()

    member this.ViewMatrix with get () = view
    member this.Position with get () = pos and set (value:Vector3) = pos <- value; UpdateViewPosition()