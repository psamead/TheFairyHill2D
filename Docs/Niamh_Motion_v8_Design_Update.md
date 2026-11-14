# Niamh Motion v8 — maintainer sync proposal, 2026-11-14

Purpose: factual Unity result and reusable lessons for the owner and future agents. The design repository is read-only for this worker; the maintainer can reconcile this note into `TechnicalArt/CH1_UNITY_PROGRESS.md`, `TechnicalArt/LESSONS_CHARACTER_ANIMATION.md` and `ART_HANDOFF.md`. This proposal is not a claim that those files have been updated.

Source: pre-staged `Motion_v8_Source/` six RGBA 327 × 699 PNGs and `motion_preview_spec.json`, derived from the owner's `_m` artwork. No art was downloaded or recopied. The offline GIF remains a reference only.

## Evidence and remaining acceptance

- Imported and integrated: Unity-generated Sprite metadata, separate transform-puppet prefab, seven Animator states, small presentation component and Core-only child hookup. Eight renderers include a derived, darker far leg/boot. Original collider, player root, controls, loader, lighting and static rollback renderer are retained.
- Compiled: actual Unity 6000.3.24f1 Editor reports successful script compilation. No automated test suite or temporary project was used, per the current rules.
- Play Mode observed: Village idle and rendering at game scale; Forest Core persistence and lighting; live walking, both facings, ascent/apex/descent/landing and recovery. Evidence: `Docs/References/Niamh_Motion_v8/Unity_Trial/` in the Unity repository. Initial Forest load was invoked through the existing loader, so that switch alone does not validate an exit trigger.
- Owner acceptance: owner finished and said “除了些卡顿，可能是各种软件一起运行的问题，其他基本ok”. The trial is basically accepted with stutter feedback; neither multiple-application contention nor capture overhead was profiled. The live log subsequently recorded the owner's Village → Forest switch. Run/5.6u/s and gate toggles were not independently captured. Do not mark the full animation task or 22-bone rig complete.

## Lessons proposed for reuse

1. Same-canvas parts still need correct pivot parenting. Convert top-left landmarks to Unity bottom-left pixels, put each joint at that location, and offset artwork back by the same landmark. This preserves the common origin while rotations occur around the intended joint. Here scale 0.30 × 699/100 keeps the approximately 2.1u canvas height; those numbers are project-specific.
2. Independent SpriteRenderer orders can accidentally cross foreground layers. A SortingGroup at the original actor order preserves environment occlusion while internal orders separate body parts. Here original order 10 stays behind foreground gate order 12.
3. Flip the complete visual hierarchy, including joint locations; flipping each individual part around its own pivot can pull the assembly apart. The old renderer can remain the controller's facing reference while hidden.
4. A grounded read-only property from the existing controller avoids duplicating ground casts or jump physics in the animation layer. Interruptible presentation transitions must never lock movement input.
5. Rotating whole boots changes floor contact. For this trial, the generator reads boot alpha edge points and bakes a vertical correction into walk/run clips; this helps keep the lowest sole at ground height. It is not planted-foot IK or proof that foot sliding is visually acceptable.
6. Unity preview scenes need their scene culling mask applied to a temporary capture camera. An all-magenta first render meant the camera saw no preview objects; it was not missing artwork. Setting the preview scene's mask produced the saved diagnostic sheet. Inspect the actual image before treating successful capture execution as evidence.
7. Existing TMP caches may change during Editor work even if they are unrelated to the feature. Hash known dirty files before starting; keep cache changes outside the feature commit and disclose observed differences. The Village scene and unrelated docs were not reset or saved by this trial.

The full result, controls, limitations, and rollback are in `Docs/Niamh_Motion_v8_Unity_Handoff.md` in the Unity repository.
