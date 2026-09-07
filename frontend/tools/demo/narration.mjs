// Script is also the source for captions and delivered transcripts.
export const narration = {
  marketing: [
    [
      'welcome',
      'Build in rhythm',
      'Welcome to Liturgy, a workspace for teams seeking to build technology through prayer, discernment, and service to their neighbours.',
    ],
    [
      'purpose',
      'A shared way of working',
      'The public introduction brings the purpose into focus. Our process forms our team, so the rhythm of our work deserves the same care as the things we build.',
    ],
    [
      'rhythm',
      'Discover · Discern · Develop · Demonstrate',
      'The four D cycle moves from Discover to Discern, Develop, and Demonstrate. Checklists give each transition a visible boundary, inviting teams to prepare together before moving forward.',
    ],
    [
      'board',
      'Keep the ritual',
      'Inside Develop, the board connects practical delivery with the five R loop: Request, Receive, Review, Render, and Rejoice. Each movement makes space for attention, action, and thanksgiving.',
    ],
    [
      'overview',
      'Take the overview with you',
      'The product overview has downloaded successfully as a PDF. It gives the team a shared introduction to discuss before beginning a project together.',
    ],
    [
      'signin',
      'Enter the workspace',
      'The sign in link opens the working application. In the next demonstration, we will follow a seeded project and see the rhythm enforced through real changes.',
    ],
    [
      'close',
      'Build with care',
      'Liturgy invites us to connect what we build with how we serve. Let prayer shape our attention, discernment guide our choices, and gratitude accompany the work.',
    ],
  ],
  workspace: [
    [
      'welcome',
      'Liturgy · Team workspace',
      'Welcome to the Liturgy workspace. This demonstration uses fictional projects and seeded team members, with real local services saving every change we make.',
    ],
    [
      'dashboard',
      'Begin with the team',
      'The dashboard brings the team and its projects into view. For a Christian team, shared attention begins with people: who we are serving, and what faithful work asks of us today.',
    ],
    [
      'projects',
      'Choose a project',
      'Here are the projects in the demonstration workspace. We will follow Lantern, an example project about an after hours crisis line. These are synthetic records, not live service data.',
    ],
    [
      'journey',
      'The four D journey',
      'Lantern is in Develop. Discover and Discern are already complete in this seeded example. Demonstrate remains behind a gate whose checklist the team can inspect.',
    ],
    [
      'gate',
      'A visible boundary',
      'The outstanding requirement is to prepare a demo for the community. This is a team maintained checklist: the server enforces its recorded state, rather than independently judging the quality of the work.',
    ],
    [
      'board',
      'Develop together',
      'The board brings work items into a shared view. We will open Lantern twenty four and finish the remaining movements in its five R loop.',
    ],
    [
      'loop',
      'Three of five movements complete',
      'This card already has Request, Receive, and Review logged. Mark done is disabled because Render and Rejoice remain. Completion asks us to carry the work through its whole rhythm.',
    ],
    [
      'render-intro',
      'Render · Give the work form',
      'Render records the artifact and what changed from the vision. We will describe a synthetic handoff script and the clearer escalation path created for this demonstration.',
    ],
    [
      'render-filled',
      'Record the result',
      'The artifact reference and explanation are now entered. These notes help the team connect its earlier discernment with the concrete work it is ready to share.',
    ],
    [
      'render-saved',
      'Render recorded',
      'Render has been saved, and Rejoice is now the current movement. The remaining step gives the team a place to name its gratitude before calling the work done.',
    ],
    [
      'rejoice',
      'Rejoice · Give thanks',
      'We give thanks to God for the people who serve their neighbours and for the care reflected in this example. Thanksgiving is recorded alongside the practical work.',
    ],
    [
      'complete',
      'Five of five movements complete',
      'All five movements are now recorded. Mark done is enabled. The application has verified the loop is complete before offering this final action.',
    ],
    [
      'done',
      'Done, and saved',
      'The card is now in Done, and it remains there after reloading the board. This result comes from the persisted application state.',
    ],
    [
      'return-gate',
      'Prepare to demonstrate',
      'We return to the project journey. With our example walkthrough prepared, we can record the outstanding community demo requirement on the checklist.',
    ],
    [
      'gate-open',
      'The checklist gate opens',
      'The gate now shows Open and keeps that state after a reload. This verifies the checklist update was saved; an open gate is distinct from claiming the project has changed phase.',
    ],
    [
      'close',
      'A rhythm of faithful work',
      'We have completed a work item and opened a checklist gate through the real application. May our work be shaped by prayer, attentive discernment, service to our neighbours, and thanksgiving.',
    ],
  ],
  api: [
    [
      'welcome',
      'Liturgy · API enforcement',
      'This technical demonstration follows real requests to the local Liturgy API. The display shows selected response fields, while authentication credentials remain outside the recording.',
    ],
    [
      'projects',
      'Read the workspace',
      'An authenticated request has returned the seeded projects. We select Lantern and inspect its actual board and movement state, using a fresh database for this demonstration.',
    ],
    [
      'loop',
      'Inspect the loop',
      'Lantern twenty four has three movements recorded and cannot yet be marked done. The response exposes both the next movement and whether completion is currently allowed.',
    ],
    [
      'reject',
      'An incomplete loop is rejected',
      'A premature Done request returned conflict, with the explanation that the five R loop is incomplete. This is server enforcement, independent of the disabled button in the browser.',
    ],
    [
      'render',
      'Record Render',
      'The Render request succeeded. The response now reports four completed movements and Rejoice as the next step. The artifact and change notes accompany the movement.',
    ],
    [
      'rejoice',
      'Record Rejoice',
      'The thanksgiving request succeeded. All five movements are logged, and the API now reports that this card can be marked done.',
    ],
    [
      'done',
      'Complete and read back',
      'The Done request succeeded, and a fresh board request confirms the card remains in Done. This read back verifies the saved result rather than relying only on the command response.',
    ],
    [
      'gate-before',
      'Inspect the gate checklist',
      'The project response shows the Develop gate is blocked by its outstanding demo preparation requirement. The checklist is a recorded team declaration, not an automated assessment of every artifact.',
    ],
    [
      'gate-after',
      'Save and verify the gate',
      'The checklist update succeeded, and another project request confirms the gate is Open. The response gives clients the same persisted gate state.',
    ],
    [
      'close',
      'Shared rules, faithful practice',
      'The API has demonstrated movement ordering, completion enforcement, and a persisted gate update. These shared rules support a team rhythm of prayer, discernment, action, and gratitude.',
    ],
  ],
};
