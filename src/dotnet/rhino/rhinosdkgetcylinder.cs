#pragma warning disable 1591
#if RHINO_SDK
using System;

namespace Rhino.Input.Custom
{
  public enum CylinderConstraint
  {
    None = 0,
    Vertical = 1,
    AroundCurve = 2
  }

  public class GetCylinder : IDisposable
  { 
    IntPtr m_ptr_argsrhinogetcylinder;
    public GetCylinder()
    {
      m_ptr_argsrhinogetcylinder = UnsafeNativeMethods.CArgsRhinoGetCylinder_New();
    }

    IntPtr ConstPointer() { return m_ptr_argsrhinogetcylinder; }
    IntPtr NonConstPointer() { return m_ptr_argsrhinogetcylinder; }

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~GetCylinder()
    {
      Dispose(false);
    }

    /// <summary>
    /// Actively reclaims unmanaged resources that this instance uses.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// For derived class implementers.
    /// <para>This method is called with argument true when class user calls Dispose(), while with argument false when
    /// the Garbage Collector invokes the finalizer, or Finalize() method.</para>
    /// <para>You must reclaim all used unmanaged resources in both cases, and can use this chance to call Dispose on disposable fields if the argument is true.</para>
    /// <para>Also, you must call the base virtual method within your overriding method.</para>
    /// </summary>
    /// <param name="disposing">true if the call comes from the Dispose() method; false if it comes from the Garbage Collector finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_ptr_argsrhinogetcylinder)
      {
        UnsafeNativeMethods.CArgsRhinoGetCylinder_Delete(m_ptr_argsrhinogetcylinder);
        m_ptr_argsrhinogetcylinder = IntPtr.Zero;
      }
    }

    /// <summary>
    /// State of the cone/cyl constraint option. When the cone/cyl option is
    /// selected, the circle is being made as a base for a cone/cyl.
    /// By default the vertical cone/cyl option not available but is not
    /// selected.  By default the "Vertical" option applies to VerticalCircle.
    /// </summary>
    public CylinderConstraint CylinderConstraint
    {
      get
      {
        IntPtr const_ptr_this = ConstPointer();
        var rc = UnsafeNativeMethods.CArgsRhinoGetCircle_ConeCylConstraint(const_ptr_this);
        if (rc == UnsafeNativeMethods.GetConeConstraint.AroundCurve)
          return CylinderConstraint.AroundCurve;
        if (rc == UnsafeNativeMethods.GetConeConstraint.Vertical)
          return CylinderConstraint.Vertical;
        return CylinderConstraint.None;
      }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.CArgsRhinoGetCircle_SetConeCylConstraint(ptr_this, (UnsafeNativeMethods.GetConeConstraint)value);
      }
    }

    /// <summary>
    /// Default radius or diameter (based on InDiameterMode)
    /// </summary>
    public double DefaultSize
    {
      get
      {
        IntPtr const_ptr_this = ConstPointer();
        return UnsafeNativeMethods.CArgsRhinoGetCircle_DefaultSize(const_ptr_this);
      }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.CArgsRhinoGetCircle_SetDefaultSize(ptr_this, value);
      }
    }

    /// <summary>
    /// Determines if the "size" value is reperesenting a radius or diameter
    /// </summary>
    public bool InDiameterMode
    {
      get { return GetBool(UnsafeNativeMethods.ArgsGetCircleBoolConsts.UseDiameterMode); }
      set { SetBool(UnsafeNativeMethods.ArgsGetCircleBoolConsts.UseDiameterMode, value); }
    }

    /// <summary>
    /// Determine if the "both sides" option is enabled
    /// </summary>
    public bool BothSidesOption
    {
      get
      {
        IntPtr const_ptr_this = ConstPointer();
        return UnsafeNativeMethods.CArgsRhinoGetCylinder_BothSides(const_ptr_this);
      }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.CArgsRhinoGetCylinder_SetBothSides(ptr_this, value);
      }
    }

    /// <summary> Height of cylinder </summary>
    public double Height
    {
      get
      {
        IntPtr const_ptr_this = ConstPointer();
        return UnsafeNativeMethods.CArgsRhinoGetCylinder_Height(const_ptr_this);
      }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.CArgsRhinoGetCylinder_SetHeight(ptr_this, value);
      }
    }

    bool GetBool(UnsafeNativeMethods.ArgsGetCircleBoolConsts which)
    {
      IntPtr const_ptr_this = ConstPointer();
      return UnsafeNativeMethods.CArgsRhinoGetCircle_GetBool(const_ptr_this, which);
    }
    void SetBool(UnsafeNativeMethods.ArgsGetCircleBoolConsts which, bool value)
    {
      IntPtr ptr_this = NonConstPointer();
      UnsafeNativeMethods.CArgsRhinoGetCircle_SetBool(ptr_this, which, value);
    }

    public bool Cap
    {
      get { return GetBool(UnsafeNativeMethods.ArgsGetCircleBoolConsts.Cap); }
      set { SetBool(UnsafeNativeMethods.ArgsGetCircleBoolConsts.Cap, value); }
    }

    /// <summary> Perform the 'get' operation. </summary>
    /// <param name="cylinder"></param>
    /// <returns></returns>
    public Commands.Result Get(out Geometry.Cylinder cylinder)
    {
      IntPtr ptr_this = NonConstPointer();
      cylinder = Geometry.Cylinder.Unset;
      uint rc = UnsafeNativeMethods.RHC_RhinoGetCylinder(ref cylinder, ptr_this);
      return (Commands.Result)rc;
    }
  }
}
#endif