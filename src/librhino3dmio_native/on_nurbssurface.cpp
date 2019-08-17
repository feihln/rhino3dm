#include "stdafx.h"

RH_C_FUNCTION ON_NurbsSurface* ON_NurbsSurface_New(int dimension, bool isRational, int order0, int order1, int cvCount0, int cvCount1)
{
  return ON_NurbsSurface::New(dimension, isRational ? TRUE : FALSE, order0, order1, cvCount0, cvCount1);
}

RH_C_FUNCTION ON_NurbsSurface* ON_NurbsSurface_New2(const ON_NurbsSurface* pConstNurbsSurface)
{
  if (pConstNurbsSurface)
    return ON_NurbsSurface::New(*pConstNurbsSurface);
  return ON_NurbsSurface::New();
}

RH_C_FUNCTION void ON_NurbsSurface_CopyFrom(const ON_NurbsSurface* pConstSourceNurbsSurface, ON_NurbsSurface* pDestNurbsSurface)
{
  if (pConstSourceNurbsSurface && pDestNurbsSurface)
    *pDestNurbsSurface = *pConstSourceNurbsSurface;
}

RH_C_FUNCTION bool ON_NurbsSurface_GetBoolDir(ON_NurbsSurface* pSurface, int which, int dir)
{
  const int idxIsClampedStart = 1;
  const int idxIsClampedEnd = 2;
  const int idxClampStart = 4;
  const int idxClampEnd = 5;
  bool rc = false;
  if (pSurface)
  {
    switch (which)
    {
    case idxIsClampedStart:
      rc = pSurface->IsClamped(dir, 0);
      break;
    case idxIsClampedEnd:
      rc = pSurface->IsClamped(dir, 1);
      break;
    case idxClampStart:
      rc = pSurface->ClampEnd(dir, 0);
      break;
    case idxClampEnd:
      rc = pSurface->ClampEnd(dir, 1);
      break;
    default:
      break;
    }
  }
  return rc;
}

RH_C_FUNCTION double ON_NurbsSurface_SuperfluousKnot(const ON_NurbsSurface* pConstNurbsSurface, int dir, int end)
{
  double rc = 0;
  if (pConstNurbsSurface)
    rc = pConstNurbsSurface->SuperfluousKnot(dir, end);
  return rc;
}

RH_C_FUNCTION bool ON_NurbsSurface_GetBool(ON_NurbsSurface* pSurface, int which)
{
  const int idxIsRational = 0;
  const int idxZeroCVs = 3;
  const int idxMakeRational = 6;
  const int idxMakeNonRational = 7;
  bool rc = false;
  if (pSurface)
  {
    switch (which)
    {
    case idxIsRational:
      rc = pSurface->IsRational();
      break;
    case idxZeroCVs:
      rc = pSurface->ZeroCVs();
      break;
    case idxMakeRational:
      rc = pSurface->MakeRational();
      break;
    case idxMakeNonRational:
      rc = pSurface->MakeNonRational();
      break;
    default:
      break;
    }
  }
  return rc;
}

RH_C_FUNCTION bool ON_NurbsSurface_IncreaseDegree(ON_NurbsSurface* pSurface, int dir, int desiredDegree)
{
  bool rc = false;
  if (pSurface)
  {
    rc = pSurface->IncreaseDegree(dir, desiredDegree);
  }
  return rc;
}

RH_C_FUNCTION int ON_NurbsSurface_GetIntDir(const ON_NurbsSurface* pSurface, int which, int dir)
{
  const int idxOrder = 1;
  const int idxCVCount = 2;
  const int idxKnotCount = 3;
  int rc = 0;
  if (pSurface)
  {
    switch (which)
    {
    case idxOrder:
      rc = pSurface->Order(dir);
      break;
    case idxCVCount:
      rc = pSurface->CVCount(dir);
      break;
    case idxKnotCount:
      rc = pSurface->KnotCount(dir);
      break;
    default:
      break;
    }
  }
  return rc;
}

//RH_C_FUNCTION int ON_NurbsSurface_GetInt( const ON_NurbsSurface* pSurface, int which )
//{
//  const int idxCVSize = 0;
//  const int idxCVCount = 2;
//  const int idxCVStyle = 4;
//  int rc = 0;
//  if( pSurface )
//  {
//    switch(which)
//    {
//    case idxCVSize:
//      rc = pSurface->CVSize();
//      break;
//    case idxCVCount:
//      rc = pSurface->CVCount();
//      break;
//    case idxCVStyle:
//      rc = pSurface->CVStyle();
//      break;
//    }
//  }
//  return rc;
//}


RH_C_FUNCTION bool ON_NurbsSurface_GetGrevillePoint(const ON_NurbsSurface* pConstNurbsSurface, int u, int v, ON_2dPoint* point)
{
  bool rc = false;
  if (pConstNurbsSurface && point)
  {
    double gu = pConstNurbsSurface->GrevilleAbcissa(0, u);
    double gv = pConstNurbsSurface->GrevilleAbcissa(1, v);

    point->x = gu;
    point->y = gv;

    rc = true;
  }
  return rc;
}

RH_C_FUNCTION bool ON_NurbsSurface_GetCV3(const ON_NurbsSurface* pSurface, int u, int v, ON_3dPoint* pPoint)
{
  // 7-Jun-2017 Dale Fugier, https://mcneel.myjetbrains.com/youtrack/issue/RH-39466
  bool rc = false;
  if (
    nullptr != pSurface &&
    nullptr != pPoint &&
    u >= 0 && u < pSurface->CVCount(0) &&
    v >= 0 && v < pSurface->CVCount(1)
    )
  {
    rc = pSurface->GetCV(u, v, *pPoint);
  }
  return rc;
}

RH_C_FUNCTION bool ON_NurbsSurface_SetCV3(ON_NurbsSurface* pSurface, int u, int v, ON_3dPoint* pPoint)
{
  // 7-Jun-2017 Dale Fugier, https://mcneel.myjetbrains.com/youtrack/issue/RH-39466
  bool rc = false;
  if (
    nullptr != pSurface &&
    nullptr != pPoint &&
    u >= 0 && u < pSurface->CVCount(0) &&
    v >= 0 && v < pSurface->CVCount(1)
    )
  {
    rc = pSurface->SetCV(u, v, *pPoint);
  }
  return rc;
}

RH_C_FUNCTION bool ON_NurbsSurface_GetCV4(const ON_NurbsSurface* pSurface, int u, int v, ON_4dPoint* pPoint)
{
  // 7-Jun-2017 Dale Fugier, https://mcneel.myjetbrains.com/youtrack/issue/RH-39466
  bool rc = false;
  if (
    nullptr != pSurface &&
    nullptr != pPoint &&
    u >= 0 && u < pSurface->CVCount(0) &&
    v >= 0 && v < pSurface->CVCount(1)
    )
  {
    rc = pSurface->GetCV(u, v, *pPoint);
  }
  return rc;
}

RH_C_FUNCTION bool ON_NurbsSurface_SetCV4(ON_NurbsSurface* pSurface, int u, int v, ON_4dPoint* pPoint)
{
  // 7-Jun-2017 Dale Fugier, https://mcneel.myjetbrains.com/youtrack/issue/RH-39466
  bool rc = false;
  if (
    nullptr != pSurface &&
    nullptr != pPoint &&
    u >= 0 && u < pSurface->CVCount(0) &&
    v >= 0 && v < pSurface->CVCount(1)
    )
  {
    // This is basically what ON_NurbsSurface::SetWeight does.
    // 26-June-2017 David Rutten, only attempt to make the surface rational if the weight is not 1.0
    if (pPoint->w != 1.0)
      if (0 == pSurface->m_is_rat && pPoint->w > 0.0 && pPoint->w < ON_UNSET_POSITIVE_VALUE)
        pSurface->MakeRational();

    rc = pSurface->SetCV(u, v, *pPoint);
  }

  return rc;
}

RH_C_FUNCTION double  ON_NurbsSurface_Weight(ON_NurbsSurface* pSurface, int u, int v)
{
  double rc = ON_UNSET_VALUE;
  if (
    nullptr != pSurface &&
    u >= 0 && u < pSurface->CVCount(0) &&
    v >= 0 && v < pSurface->CVCount(1)
    )
  {
    rc = pSurface->Weight(u, v);
  }
  return rc;
}

RH_C_FUNCTION bool ON_NurbsSurface_SetWeight(ON_NurbsSurface* pSurface, int u, int v, double weight)
{
  bool rc = false;
  if (nullptr != pSurface)
    rc = pSurface->SetWeight(u, v, weight);
  return rc;
}

RH_C_FUNCTION int ON_NurbsSurface_CVSize(ON_NurbsSurface* pSurface)
{
  int rc = 0;
  if (nullptr != pSurface)
    rc = pSurface->CVSize();
  return rc;
}

RH_C_FUNCTION bool ON_NurbsSurface_SetKnot(ON_NurbsSurface* pSurface, int dir, int knotIndex, double knotValue)
{
  bool rc = false;
  if (pSurface)
  {
    rc = pSurface->SetKnot(dir, knotIndex, knotValue) ? true : false;
  }
  return rc;
}

RH_C_FUNCTION double ON_NurbsSurface_Knot(const ON_NurbsSurface* pSurface, int dir, int knotIndex)
{
  double rc = 0;
  if (pSurface)
    rc = pSurface->Knot(dir, knotIndex);
  return rc;
}

RH_C_FUNCTION int ON_NurbsSurface_KnotMultiplicity(const ON_NurbsSurface* pSurface, int dir, int knotIndex)
{
  int rc = 0;
  if (pSurface)
    rc = pSurface->KnotMultiplicity(dir, knotIndex);
  return rc;
}

RH_C_FUNCTION bool ON_NurbsSurface_MakeUniformKnotVector(ON_NurbsSurface* pSurface, int dir, double delta, bool clamped)
{
  bool rc = false;
  if (pSurface)
  {
    if (clamped)
      rc = pSurface->MakeClampedUniformKnotVector(dir, delta);
    else
      rc = pSurface->MakePeriodicUniformKnotVector(dir, delta);
  }
  return rc;
}

//RH_C_FUNCTION double ON_NurbsSurface_GrevilleAbcissa(const ON_NurbsSurface* pSurface, int dir, int index)
//{
//  double rc = 0;
//  if( pSurface )
//    rc = pSurface->GrevilleAbcissa(dir, index);
//  return rc;
//}

//RH_C_FUNCTION bool ON_NurbsSurface_GetGrevilleAbcissae(const ON_NurbsSurface* pSurface, int dir, double* ga)
//{
//  bool rc = false;
//  if( pSurface && ga )
//    rc = pSurface->GetGrevilleAbcissae(dir, ga);
//  return rc;
//}

RH_C_FUNCTION bool ON_NurbsSurface_InsertKnot(ON_NurbsSurface* pSurface, int dir, double knotValue, int knotMultiplicity)
{
  bool rc = false;
  if (pSurface)
  {
    rc = pSurface->InsertKnot(dir, knotValue, knotMultiplicity);
  }
  return rc;
}

RH_C_FUNCTION ON_NurbsSurface* ON_NurbsSurface_CreateRuledSurface(const ON_Curve* pConstA, const ON_Curve* pConstB)
{
  ON_NurbsSurface* rc = nullptr;

  if (pConstA && pConstB)
  {
    // Always use static constructor
    //rc = new ON_NurbsSurface();
    rc = ON_NurbsSurface::New();
    rc->CreateRuledSurface(*pConstA, *pConstB);
    if (!rc->IsValid())
    {
      delete rc;
      rc = nullptr;
    }
  }

  return rc;
}

RH_C_FUNCTION ON_MorphControl* ON_MorphControl_New(const ON_MorphControl* pConstOther)
{
  if (pConstOther)
    return new ON_MorphControl(*pConstOther);
  return new ON_MorphControl();
}

RH_C_FUNCTION void ON_MorphControl_SetCurves(ON_MorphControl* pMorphControl, const ON_NurbsCurve* pConstNurbsCurve0, const ON_NurbsCurve* pConstNurbsCurve1)
{
  if (pMorphControl && pConstNurbsCurve0 && pConstNurbsCurve1)
  {
    pMorphControl->m_varient = 1;
    pMorphControl->m_nurbs_curve0 = *pConstNurbsCurve0;
    pMorphControl->m_nurbs_curve = *pConstNurbsCurve1;
  }
}

RH_C_FUNCTION double ON_MorphControl_GetSporhTolerance(const ON_MorphControl* pConstMorphControl)
{
  double rc = 0;
  if (pConstMorphControl)
    rc = pConstMorphControl->m_sporh_tolerance;
  return rc;
}

RH_C_FUNCTION void ON_MorphControl_SetSporhTolerance(ON_MorphControl* pMorphControl, double tolerance)
{
  if (pMorphControl)
    pMorphControl->m_sporh_tolerance = tolerance;
}

RH_C_FUNCTION const ON_NurbsCurve* ON_MorphControl_GetCurve(const ON_MorphControl* pConstMorphControl)
{
  // 23 March 2018, Mikko, RH-44990:
  // Fixed crash in MoveUVN. The returned pointer is being used as non-const, and will cause double delete crash
  // later on. Looks like elsewhere this is handled by basically ignoring the word "const" before ON_NurbsCurve 
  // and always returning a duplicate, so for consistency I'm doing the same here. 
  // See for example ON_V6_Leader_Curve.
  if (pConstMorphControl)
    return pConstMorphControl->m_nurbs_curve.Duplicate();
  return nullptr;
}

RH_C_FUNCTION const ON_NurbsSurface* ON_MorphControl_GetSurface(const ON_MorphControl* pConstMorphControl)
{
  // 23 March 2018, Mikko, RH-44990:
  // Fixed crash in MoveUVN. The returned pointer is being used as non-const, and will cause double delete crash
  // later on. Looks like elsewhere this is handled by basically ignoring the word "const" before ON_NurbsSurface 
  // and always returning a duplicate, so for consistency I'm doing the same here.
  // See for example ON_V6_Leader_Curve.
  if (pConstMorphControl)
    return pConstMorphControl->m_nurbs_surface.Duplicate();
  return nullptr;
}

RH_C_FUNCTION bool ON_MorphControl_GetBool(const ON_MorphControl* pConstMorphControl, bool quickpreview)
{
  bool rc = false;
  if (pConstMorphControl)
    rc = quickpreview ? pConstMorphControl->m_sporh_bQuickPreview : pConstMorphControl->m_sporh_bPreserveStructure;
  return rc;
}

RH_C_FUNCTION void ON_MorphControl_SetBool(ON_MorphControl* pMorphControl, bool val, bool quickpreview)
{
  if (pMorphControl)
  {
    if (quickpreview)
      pMorphControl->m_sporh_bQuickPreview = val;
    else
      pMorphControl->m_sporh_bPreserveStructure = val;
  }
}

// not currently available in stand alone OpenNURBS build
#if !defined(RHINO3DMIO_BUILD)

RH_C_FUNCTION bool ON_MorphControl_MorphGeometry(const ON_MorphControl* pConstMorphControl, ON_Geometry* pGeometry)
{
  bool rc = false;
  if (pConstMorphControl && pGeometry)
  {
    ON_CageMorph cage_morph;
    if (pConstMorphControl->GetCageMorph(cage_morph))
    {
      rc = pGeometry->Morph(cage_morph);
    }
  }
  return rc;
}

#endif